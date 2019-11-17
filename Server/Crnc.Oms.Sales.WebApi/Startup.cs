using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.Application.Features.Orders.Queries;
using Crnc.Oms.Sales.DataAccess;
using Crnc.Oms.Sales.WebApi.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using NSwag;
using Newtonsoft.Json.Converters;

namespace Crnc.Oms.Sales.WebApi
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
            services.AddCors(options => {
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
            services.AddDbContext<SalesDataContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("OmsSalesDb"));
            });
            services.AddScoped<IUseCaseQueryHandler<OrdersForTableInputDto, OrdersForTableOutputDto>,GetOrdersForTable>();
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
                options.Title = "Crnc Oms Sales API Doc";
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SalesDataContext dbContext)
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

            SalesDbInitializer.Initialize(dbContext);
        }
    }
}