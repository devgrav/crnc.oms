using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Security.Domain.Repositories;
using Crnc.Oms.Security.Domain.SeedWork;
using Crnc.Oms.Security.Infrastructure.DataAccess;
using Crnc.Oms.Security.Infrastructure.DataAccess.Cache;
using Crnc.Oms.Security.Infrastructure.DataAccess.Repositories;
using Crnc.Oms.Security.WebApi.Authorization;
using Crnc.Oms.Security.WebApi.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// ---- Services -------------------------------------------------------

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllOrigins", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddOptions();
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("ConnectionStrings:OmsSecurityDb"));
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));

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
    // Title in header of api
    options.Title = "Crnc Oms Security API Doc";
    // Version in header of api
    options.Version = "1.0";
    options.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Please insert JWT with Bearer into field. Example: Bearer {your token}"
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Preserve the exact wire format the SPA expects: camelCase property names, camelCase
    // dictionary keys (STJ's PropertyNamingPolicy alone does NOT cover dictionary keys -
    // needed for BadRequest(ModelState) validation error responses), case-insensitive
    // deserialization (Newtonsoft's default), and string enums.
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var cacheSettings = new CacheSettings();
builder.Configuration.GetSection("Cache").Bind(cacheSettings);

if (cacheSettings.IsUse)
{
    builder.Services.AddScoped<IEntityCollectionCacheProvider<User>, MongoInMemoryEntityCollectionCacheProvider<User>>();
    builder.Services.AddScoped<IEntityCollectionCacheProvider<Role>, MongoInMemoryEntityCollectionCacheProvider<Role>>();
    builder.Services.AddScoped<IUserRepository, CachedMongoDbUserRepository>();
    builder.Services.AddMemoryCache();
}
else
{
    builder.Services.AddScoped<IUserRepository, MongoDbUserRepository>();
}

builder.Services.AddSingleton<ICurrentDateTimeProvider, CurrentDateTimeProvider>();
builder.Services.AddSingleton<MongoDataContext>();

// ---- Pipeline ---------------------------------------------------------

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseMonitoringRequestMiddleware();

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

// Drops and reseeds the entire Mongo DB with Bogus fake data on every boot, before the
// app starts serving traffic.
var mongoDataContext = app.Services.GetRequiredService<MongoDataContext>();
await new MongoDbInitializer(mongoDataContext).InitializeAsync();

app.Run();
