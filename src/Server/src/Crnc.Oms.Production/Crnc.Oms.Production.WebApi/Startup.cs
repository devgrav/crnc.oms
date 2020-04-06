using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Messaging.Contract.Commands;
using Crnc.Oms.Production.Domain.SeedWork;
using Crnc.Oms.Production.Application;
using Crnc.Oms.Production.Application.Services;
using Crnc.Oms.Production.Application.Services.Abstractions;
using Crnc.Oms.Production.DataAccess;
using Crnc.Oms.Production.DataAccess.Repositories;
using Crnc.Oms.Production.Domain.Repositories;
using Crnc.Oms.Production.WebApi.Middlewares;
using Crnc.Oms.Production.WebApi.Authorization;
using Crnc.Oms.Production.WebApi.Settings;
using GreenPipes;
using MassTransit;
using MassTransit.AspNetCoreIntegration;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using NSwag;
using Newtonsoft.Json.Converters;
using Prometheus;

namespace Crnc.Oms.Production.WebApi
{
    public class Startup
    {
        public ILoggerFactory LoggerFactory { get; }
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            
            services.AddCors(options => {
                options.AddPolicy("AllOrigins", builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });
            
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            services.AddDbContext<ProductionDataContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("OmsProductionDb"));
            });

            services.AddMassTransit(ConfigureBus(), LoggerFactory);

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            services.AddScoped<ICurrentUserContext, CurrentUserContext>();

            services.AddSingleton<ICurrentDateTimeProvider, CurrentDateTimeProvider>();
            
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IJobRepository, JobRepository>();

            services.Configure<AuthSettings>(Configuration.GetSection("Auth"));
            
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
            
            services.AddOpenApiDocument(options =>
            {
                //Title in header of api
                options.Title = "Crnc Oms Production API Doc";
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
        
        IBusControl ConfigureBus() => Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            var integrationSettings = new IntegrationEndpointSettings();
            Configuration.GetSection("IntegrationEndpoints").Bind(integrationSettings);
            
            cfg.Host(integrationSettings.MessageBrokerEndpoint);

            EndpointConvention.Map<OrderConvertedToJobEvent>(new Uri($"{integrationSettings.MessageBrokerEndpoint}/commands/sendNotificationToUser"));
        });
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ProductionDataContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseHealthChecks("/health", new HealthCheckOptions {Predicate = check => check.Tags.Contains("ready")});
            
            var cultureInfo = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            app.UseMonitoringRequestMiddleware();
            
            app.UseRouting();
            app.UseHttpMetrics();
            app.UseCors("AllOrigins");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(e =>
            {
                e.MapControllers();
                e.MapMetrics();
            });

            app.UseOpenApi();
            app.UseSwaggerUi3();

            ProductionDbInitializer.Initialize(dbContext);
        }
    }
}