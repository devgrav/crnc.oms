using System.Globalization;
using System.Text.Json;
using Crnc.Oms.Messaging.Contract.Commands;
using Crnc.Oms.Sales.Application;
using Crnc.Oms.Sales.DataAccess;
using Crnc.Oms.Sales.DataAccess.Repositories;
using Crnc.Oms.Sales.Domain.Gateways;
using Crnc.Oms.Sales.Domain.Repositories;
using Crnc.Oms.Sales.Domain.SeedWork;
using Crnc.Oms.Sales.Integration.Gateways;
using Crnc.Oms.Sales.Integration.Settings;
using Crnc.Oms.Sales.WebApi.Authorization;
using Crnc.Oms.Sales.WebApi.Consumers;
using Crnc.Oms.Sales.WebApi.Middlewares;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using Prometheus;

// Риск 1: сохраняем поведение Npgsql до 6.0 - DateTime с Kind=Local/Unspecified
// пишутся в timestamp without time zone. Весь домен оперирует DateTime.Now.
// Ставить строго до создания любого NpgsqlDataSource/DbContext.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllOrigins", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    // JsonStringEnumConverter НЕ добавляем - см. план миграции §4: enum'ы в контрактах Sales
    // сейчас числовые, SPA ожидает числа.
});

builder.Services.AddDbContext<SalesDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OmsSalesDb")));

builder.Services.Configure<IntegrationEndpointSettings>(
    builder.Configuration.GetSection("IntegrationEndpoints"));

var integrationSettings = new IntegrationEndpointSettings();
builder.Configuration.GetSection("IntegrationEndpoints").Bind(integrationSettings);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<JobCreatedForOrderConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(integrationSettings.MessageBrokerEndpoint);

        cfg.ReceiveEndpoint("jobCreatedForOrder", e =>
        {
            e.ConfigureConsumer<JobCreatedForOrderConsumer>(context);
        });

        EndpointConvention.Map<SendNotificationToUserCommand>(
            new Uri($"{integrationSettings.MessageBrokerEndpoint}/commands/sendNotificationToUser"));
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(IDomainEventNotificationHandler).Assembly));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Переход с RestSharp на типизированный HttpClient (устранение риска сломанного API RestSharp 106->112+).
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddHttpClient<IEmployeeGateway, EmployeeSecurityGateway>(client =>
    client.BaseAddress = new Uri(integrationSettings.SecurityServiceEndpoint.TrimEnd('/') + "/"));

builder.Services.AddScoped<INotificationGateway, MessageBrokerNotificationGateway>();
builder.Services.AddScoped<IProductionJobGateway, ProductionJobGateway>();

builder.Services.AddSingleton<ICurrentDateTimeProvider, CurrentDateTimeProvider>();
builder.Services.AddScoped<ICommandQueryDispatcher, CommandQueryDispatcher>();

builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authSettings = new AuthSettings();
        builder.Configuration.GetSection("Auth").Bind(authSettings);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authSettings.JwtIssuer,
            ValidAudience = authSettings.JwtAudience,
            IssuerSigningKey = authSettings.SymmetricSecurityKey
        };
    });

builder.Services.AddOpenApiDocument(options =>
{
    options.Title = "Crnc Oms Sales API Doc";
    options.Version = "1.0";
    options.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Please insert JWT with Bearer into field"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseMonitoringRequestMiddleware();

app.UseRouting();
app.UseHttpMetrics();
app.UseCors("AllOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMetrics();
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.UseOpenApi();
app.UseSwaggerUi();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SalesDataContext>();
    SalesDbInitializer.Initialize(dbContext);
}

app.Run();
