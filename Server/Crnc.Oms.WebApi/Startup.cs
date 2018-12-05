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
using Crnc.Oms.Infrastructure.DataAccess;
using Crnc.Oms.Domain.IRepositories;
using Crnc.Oms.Infrastructure.DataAccess.Repositories;
using System.Globalization;
using Newtonsoft.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Crnc.Oms.WebApi.Authorization;
using Swashbuckle.AspNetCore.SwaggerUI;
using Swashbuckle.AspNetCore.Swagger;

namespace Crnc.Oms.WebApi
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
            services.AddCors(options => {
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
                c.SwaggerDoc("v1.0", new Info { Title = "Crnc Oms API", Version = "v1.0" });
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };
 
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(security);
            });

            services.AddMvc()
                .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());
            services.AddScoped<IUserRepository, MongoDbUserRepository>();
            services.AddSingleton<MongoDataContext>();     
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            var cultureInfo = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            app.UseCors("AllOrigins");
            app.UseAuthentication();
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api-docs";
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Crnc Oms API v1.0");
 
                c.DocumentTitle = "Crnc Oms API Doc";
                c.DocExpansion(DocExpansion.None);
            });
        }
    }
}
