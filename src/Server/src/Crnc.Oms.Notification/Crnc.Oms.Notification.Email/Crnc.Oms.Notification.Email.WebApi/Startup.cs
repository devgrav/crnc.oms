using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Messaging.Contract.Commands;
using Crnc.Oms.Notification.Gateway.Integration.Gateways;
using Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions;
using Crnc.Oms.Notification.Gateway.WebApi.Authorization;
using Crnc.Oms.Notification.Email.Application.Services;
using Crnc.Oms.Notification.Email.Application.Services.Abstractions;
using Crnc.Oms.Notification.Email.Integration.Settings;
using Crnc.Oms.Notification.Email.WebApi.Consumers;
using MassTransit;
using MassTransit.AspNetCoreIntegration;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NSwag;

namespace Crnc.Oms.Notification.Email.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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
            
            IBusControl CreateBus(IServiceProvider serviceProvider)
            {
                return Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    var integrationSettings = new IntegrationEndpointSettings();
                    Configuration.GetSection("IntegrationEndpoints").Bind(integrationSettings);
            
                    cfg.Host(integrationSettings.MessageBrokerEndpoint);
            
                    cfg.ReceiveEndpoint("sendEmailNotificationToUser", e =>
                    {
                        e.ConfigureConsumer<SendEmailNotificationToUserConsumer>(serviceProvider);
                    });
                });
            }
            
            void ConfigureMassTransit(IServiceCollectionConfigurator configurator)
            {
                configurator.AddConsumer<SendEmailNotificationToUserConsumer>();
            }
            
            services.AddMassTransit(CreateBus, ConfigureMassTransit);

            services.AddScoped<IEmailNotificationService, EmailNotificationService>();
            services.AddScoped<IEmailGateway, EmailGateway>();

            services.AddLogging();
            
            services.Configure<AuthSettings>(Configuration.GetSection("Auth"));

            services.AddOpenApiDocument(options =>
            {
                //Title in header of api
                options.Title = "Crnc Oms Email Notification API Doc";
                //Version in header of api
                options.Version = "1.0";
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