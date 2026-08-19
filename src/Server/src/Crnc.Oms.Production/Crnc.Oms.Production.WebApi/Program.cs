using System.Globalization;
using System.Text.Json;
using Crnc.Oms.Production.Application.Services;
using Crnc.Oms.Production.Application.Services.Abstractions;
using Crnc.Oms.Production.DataAccess;
using Crnc.Oms.Production.DataAccess.Repositories;
using Crnc.Oms.Production.Domain.Gateways;
using Crnc.Oms.Production.Domain.Repositories;
using Crnc.Oms.Production.Domain.SeedWork;
using Crnc.Oms.Production.Integration.Gateways;
using Crnc.Oms.Production.Integration.Settings;
using Crnc.Oms.Production.WebApi.Authorization;
using Crnc.Oms.Production.WebApi.Consumers;
using Crnc.Oms.Production.WebApi.Middlewares;
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
    // JsonStringEnumConverter НЕ добавляем - см. §4 плана: PriorityEnum сейчас числовой,
    // SPA (components/jobs/priority.ts) ожидает числа.
});

builder.Services.AddDbContext<ProductionDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OmsProductionDb")));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddSingleton<ICurrentDateTimeProvider, CurrentDateTimeProvider>();

builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<ISalesOrderGateway, SalesOrderGateway>();

builder.Services.Configure<IntegrationEndpointSettings>(
    builder.Configuration.GetSection("IntegrationEndpoints"));

var integrationSettings = new IntegrationEndpointSettings();
builder.Configuration.GetSection("IntegrationEndpoints").Bind(integrationSettings);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConvertedToJobConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(integrationSettings.MessageBrokerEndpoint);

        cfg.ReceiveEndpoint("orderConvertedToJob", e =>
        {
            e.ConfigureConsumer<OrderConvertedToJobConsumer>(context);
        });
    });
});

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
    options.Title = "Crnc Oms Production API Doc";
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
app.UseSwaggerUi(); // переименован из UseSwaggerUi3() в NSwag v14

// Существующее поведение вне скоупа миграции: полное пересоздание схемы и сида
// при каждом старте. Раньше ProductionDataContext инжектился параметром Configure(),
// теперь достаём из scope явно.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductionDataContext>();
    ProductionDbInitializer.Initialize(dbContext);
}

app.Run();
