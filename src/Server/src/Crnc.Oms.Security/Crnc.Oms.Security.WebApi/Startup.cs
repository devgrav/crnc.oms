using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Crnc.Oms.Security.Infrastructure.DataAccess;
using Crnc.Oms.Security.Domain.Repositories;
using Crnc.Oms.Security.Infrastructure.DataAccess.Repositories;
using System.Globalization;
using Newtonsoft.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Security.Domain.SeedWork;
using Crnc.Oms.Security.Infrastructure.DataAccess.Cache;
using Crnc.Oms.Security.WebApi.Authorization;
using Crnc.Oms.Security.WebApi.Middlewares;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Converters;
using NSwag;
using Prometheus;

namespace Crnc.Oms.Security.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;           
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
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
            services.AddOptions();
            services.Configure<MongoDbSettings>(Configuration.GetSection("ConnectionStrings:OmsSecurityDb"));
            services.Configure<AuthSettings>(Configuration.GetSection("Auth"));
            services.Configure<CacheSettings>(Configuration.GetSection("Cache"));
            
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
                options.Title = "Crnc Oms Security API Doc";
                //Version in header of api
                options.Version = "1.0";
                options.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Please insert JWT with Bearer into field. Example: Bearer {your token}"
                });
            });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });
            
            var cacheSettings = new CacheSettings();
            Configuration.GetSection("Cache").Bind(cacheSettings);
            
            if (cacheSettings.IsUse)
            {
                services.AddScoped<IEntityCollectionCacheProvider<User>, MongoInMemoryEntityCollectionCacheProvider<User>>();
                services.AddScoped<IEntityCollectionCacheProvider<Role>, MongoInMemoryEntityCollectionCacheProvider<Role>>();
                services.AddScoped<IUserRepository, CachedMongoDbUserRepository>();
                services.AddMemoryCache();
            }
            else
            {
                services.AddScoped<IUserRepository, MongoDbUserRepository>();
            }
            
            services.AddSingleton<ICurrentDateTimeProvider, CurrentDateTimeProvider>();
            services.AddSingleton<MongoDataContext>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MongoDataContext mongoDataContext)
        {
            if (env.IsDevelopment())
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
            app.UseEndpoints(e =>
            {
                e.MapControllers();
                e.MapMetrics();
            });

            app.UseOpenApi();
            app.UseSwaggerUi3();

            new MongoDbInitializer(mongoDataContext).Initialize();
        }
    }
}
