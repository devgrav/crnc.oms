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
using Crnc.Oms.Security.Domain.IRepositories;
using Crnc.Oms.Security.Infrastructure.DataAccess.Repositories;
using System.Globalization;
using Newtonsoft.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Crnc.Oms.Security.WebApi.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.SwaggerUI;
using Swashbuckle.AspNetCore.Swagger;

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
            services.AddCors(options =>
            {
                options.AddPolicy("AllOrigins", builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });
            services.AddOptions();
            services.Configure<MongoDbSettings>(Configuration.GetSection("ConnectionStrings:Omsdb"));
            services.Configure<AuthSettings>(Configuration.GetSection("Auth"));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var securityKey = Configuration.GetSection("Auth").GetValue<string>("JwtBase64SymmetricKey");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = AuthSettings.ISSUER,
                        ValidAudience = AuthSettings.AUDIENCE,
                        IssuerSigningKey = AuthSettings.GetSymmetricSecurityKey(securityKey)
                    };
                });

            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc("v1.0", new OpenApiInfo() {Title = "Crnc Oms Security API", Version = "v1.0"});
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header, Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization", Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });
            
            services.AddScoped<IUserRepository, MongoDbUserRepository>();
            services.AddSingleton<MongoDataContext>();     
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MongoDataContext mongoDataContext)
        {

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            var cultureInfo = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            app.UseRouting();
            app.UseCors("AllOrigins");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(e => 
                e.MapControllers());

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Crnc Oms Security API v1.0");
                c.DocumentTitle = "Crnc Oms Security API Doc";
                c.DocExpansion(DocExpansion.None);
            });
            
            new MongoDbInitializer(mongoDataContext).Initialize();
        }
    }
}
