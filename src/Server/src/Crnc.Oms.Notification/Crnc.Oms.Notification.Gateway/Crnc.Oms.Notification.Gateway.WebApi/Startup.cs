using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Gateways;
using Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions;
using Crnc.Oms.Notification.Gateway.WebApi.Authorization;
using Crnc.Oms.Notification.Gateway.Application.Services;
using Crnc.Oms.Notification.Gateway.Application.Services.Abstractions;
using Crnc.Oms.Notification.Gateway.Integration;
using Crnc.Oms.Notification.Gateway.Integration.Settings;
using Crnc.Oms.Notification.Gateway.WebApi.Consumers;
using MassTransit;
using MassTransit.AspNetCoreIntegration;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NSwag;

namespace Crnc.Oms.Notification.Gateway.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        
        public ILoggerFactory LoggerFactory { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            
            services.AddCors(options =>
            {
                options.AddPolicy("AllOrigins", builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailGateway, EmailGateway>();
            services.AddScoped<IPushGateway, PushGateway>();
            services.AddScoped<IUserInfoGateway, UserInfoGateway>();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            services.AddLogging();
            
            services.Configure<AuthSettings>(Configuration.GetSection("Auth"));
            services.Configure<IntegrationEndpointSettings>(Configuration.GetSection("IntegrationEndpoints"));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var authSettings = new AuthSettings();
                    Configuration.GetSection("Auth").Bind(authSettings);
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
            
            IBusControl CreateBus(IServiceProvider serviceProvider)
            {
                return Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    var integrationSettings = new IntegrationEndpointSettings();
                    Configuration.GetSection("IntegrationEndpoints").Bind(integrationSettings);
            
                    cfg.Host(integrationSettings.MessageBrokerEndpoint);
            
                    cfg.ReceiveEndpoint("sendNotificationToUser", e =>
                    {
                        e.ConfigureConsumer<SendNotificationToUser>(serviceProvider);
                    });
                });
            }
            
            void ConfigureMassTransit(IServiceCollectionConfigurator configurator)
            {
                configurator.AddConsumer<SendNotificationToUser>();
            }
            
            services.AddMassTransit(CreateBus, ConfigureMassTransit);

            services.AddOpenApiDocument(options =>
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
        }
        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var cultureInfo = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            app.UseRouting();
            app.UseCors("AllOrigins");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(e =>
                e.MapControllers());

            app.UseOpenApi();
            app.UseSwaggerUi3();
        }
    }
}