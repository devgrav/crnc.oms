using System.Globalization;
using System.Text.Json;
using Crnc.Oms.Notification.Email.Application.Services;
using Crnc.Oms.Notification.Email.Application.Services.Abstractions;
using Crnc.Oms.Notification.Email.Integration.Gateways;
using Crnc.Oms.Notification.Email.Integration.Gateways.Abstractions;
using Crnc.Oms.Notification.Email.Integration.Settings;
using Crnc.Oms.Notification.Email.WebApi.Authorization;
using Crnc.Oms.Notification.Email.WebApi.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
    // JsonStringEnumConverter НЕ добавляем: enum'ов в контрактах Notification нет вообще -
    // StringEnumConverter из Newtonsoft здесь был no-op. См. §4 плана миграции.
    // DictionaryKeyPolicy, наоборот, обязателен: у SendEmailMessageInputDto есть
    // [Required]/[EmailAddress], значит ключи ModelState видны снаружи и должны
    // остаться camelCase.
});

var integrationSettings = new IntegrationEndpointSettings();
builder.Configuration.GetSection("IntegrationEndpoints").Bind(integrationSettings);
builder.Services.Configure<IntegrationEndpointSettings>(
    builder.Configuration.GetSection("IntegrationEndpoints"));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SendEmailNotificationToReceiverConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(integrationSettings.MessageBrokerEndpoint);

        // Имя очереди менять нельзя: по этому адресу Gateway отправляет команды.
        cfg.ReceiveEndpoint("sendEmailNotificationToReceiver", e =>
        {
            e.ConfigureConsumer<SendEmailNotificationToReceiverConsumer>(context);
        });
    });
});

builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
builder.Services.AddScoped<IEmailGateway, EmailGateway>();

builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));

// На 3.1 Startup вызывал UseAuthentication() ни разу не вызвав AddAuthentication, и это
// сходило с рук. Схема добавляется, чтобы пайплайн собирался штатно; на доступность
// эндпойнта это не влияет - у EmailNotificationsController нет [Authorize], он был и
// остаётся анонимным (e2e это сторожит).
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
    //Title in header of api
    options.Title = "Crnc Oms Email Notification API Doc";
    //Version in header of api
    options.Version = "1.0";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// UseHttpMetrics теперь после UseRouting - на 3.1 здесь был обратный порядок, из-за
// которого метрика не видела маршрут. Gateway и Push всегда делали правильно.
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
