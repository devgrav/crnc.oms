using System.Globalization;
using System.Text.Json;
using Crnc.Oms.Notification.Push.Application.Services;
using Crnc.Oms.Notification.Push.Application.Services.Abstractions;
using Crnc.Oms.Notification.Push.Integration.Gateways;
using Crnc.Oms.Notification.Push.Integration.Gateways.Abstractions;
using Crnc.Oms.Notification.Push.Integration.Hubs;
using Crnc.Oms.Notification.Push.Integration.Settings;
using Crnc.Oms.Notification.Push.WebApi.Authorization;
using Crnc.Oms.Notification.Push.WebApi.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    var allowedOrigin = builder.Configuration.GetSection("IntegrationEndpoints:UiEndpoint").Value;

    options.AddPolicy("CorsPolicy", policy => policy
        .WithOrigins(allowedOrigin)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
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

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SendPushNotificationToUserReceiverConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(integrationSettings.MessageBrokerEndpoint);

        // Имя очереди менять нельзя: по этому адресу Gateway отправляет команды.
        cfg.ReceiveEndpoint("sendPushNotificationToReceiver", e =>
        {
            e.ConfigureConsumer<SendPushNotificationToUserReceiverConsumer>(context);
        });
    });
});

builder.Services.AddSignalR();
builder.Services.AddScoped<IPushGateway, SignalRPushGateway>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();

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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs/push")))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddOpenApiDocument(options =>
{
    //Title in header of api
    options.Title = "Crnc Oms Push Notification API Doc";
    //Version in header of api
    options.Version = "1.0";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseRouting();
app.UseHttpMetrics();
// UseCors обязан стоять до MapHub: иначе SPA перестанет подключаться к хабу,
// а Push.Client (без CORS) продолжит работать - поломка была бы видна только в браузере.
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<PushHub>("/hubs/push");
app.MapControllers();
app.MapMetrics();
app.MapHealthChecks("/health");

app.UseOpenApi();
app.UseSwaggerUi();

app.Run();
