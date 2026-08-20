using System.Globalization;
using System.Text.Json;
using Crnc.Oms.Messaging.Contract.Commands;
using Crnc.Oms.Notification.Gateway.Application.Services;
using Crnc.Oms.Notification.Gateway.Application.Services.Abstractions;
using Crnc.Oms.Notification.Gateway.Integration.Gateways;
using Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions;
using Crnc.Oms.Notification.Gateway.Integration.Settings;
using Crnc.Oms.Notification.Gateway.WebApi.Authorization;
using Crnc.Oms.Notification.Gateway.WebApi.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using Prometheus;

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
    // JsonStringEnumConverter НЕ добавляем: enum'ов в контрактах Notification нет вообще (§4 плана).
});

var integrationSettings = new IntegrationEndpointSettings();
builder.Configuration.GetSection("IntegrationEndpoints").Bind(integrationSettings);
builder.Services.Configure<IntegrationEndpointSettings>(
    builder.Configuration.GetSection("IntegrationEndpoints"));

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailGateway, MessageBrokerEmailGateway>();
builder.Services.AddScoped<IPushGateway, MessageBrokerPushGateway>();

// Переход с RestSharp на типизированный HttpClient. Токен не подставляется намеренно —
// см. комментарий в UserInfoGateway и раздел «Разрешение канала доставки» плана миграции.
builder.Services.AddHttpClient<IUserInfoGateway, UserInfoGateway>(client =>
    client.BaseAddress = new Uri(integrationSettings.SecurityServiceEndpoint.TrimEnd('/') + "/"));

builder.Services.AddHttpContextAccessor();

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

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SendNotificationToUserConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(integrationSettings.MessageBrokerEndpoint);

        // Имя очереди менять нельзя: по этому адресу Sales отправляет команды.
        cfg.ReceiveEndpoint("sendNotificationToUser", e =>
        {
            e.ConfigureConsumer<SendNotificationToUserConsumer>(context);
        });

        // Сегмент /commands/ в адресе поглощается адресацией MassTransit: команда попадает
        // на exchange, одноимённый очереди, в vhost по умолчанию. Проверено на живом стенде
        // в фазе 0 — менять адреса нельзя, на них завязаны Sales, Email и Push.
        EndpointConvention.Map<SendNotificationToUserCommand>(
            new Uri($"{integrationSettings.MessageBrokerEndpoint}/commands/sendNotificationToUser"));
        EndpointConvention.Map<SendPushNotificationToReceiverCommand>(
            new Uri($"{integrationSettings.MessageBrokerEndpoint}/commands/sendPushNotificationToReceiver"));
        EndpointConvention.Map<SendEmailNotificationToReceiverCommand>(
            new Uri($"{integrationSettings.MessageBrokerEndpoint}/commands/sendEmailNotificationToReceiver"));
    });
});

builder.Services.AddOpenApiDocument(options =>
{
    //Title in header of api
    options.Title = "Crnc Oms Notification Gateway API Doc";
    //Version in header of api
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

app.UseRouting();
app.UseHttpMetrics();
app.UseCors("AllOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMetrics();
app.MapHealthChecks("/health");

app.UseOpenApi();
app.UseSwaggerUi();

app.Run();
