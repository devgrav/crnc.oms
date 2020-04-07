using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Crnc.Oms.Production.WebApi.Middlewares
{
    public class MonitoringRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public MonitoringRequestMiddleware(
            RequestDelegate next
            , ILoggerFactory loggerFactory
        )
        {
            this._next = next;
            this._logger = loggerFactory.CreateLogger<MonitoringRequestMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.Value;
            var method = httpContext.Request.Method;

            var counter = Metrics.CreateCounter("prometheus_request_total", "HTTP Requests Total",
                new CounterConfiguration
                {
                    LabelNames = new[] {"path", "method", "status"}
                });

            var statusCode = 200;

            try
            {
                await _next.Invoke(httpContext);
            }
            catch (Exception)
            {
                statusCode = 500;
                counter.Labels(path, method, statusCode.ToString()).Inc();

                throw;
            }

            if (path != "/metrics" && path != "/favicon.ico")
            {
                statusCode = httpContext.Response.StatusCode;
                counter.Labels(path, method, statusCode.ToString()).Inc();  
            }
        }
    }
    
    public static class MonitoringRequestMiddlewareExtensions  
    {          
        public static IApplicationBuilder UseMonitoringRequestMiddleware(this IApplicationBuilder builder)  
        {  
            return builder.UseMiddleware<MonitoringRequestMiddleware>();  
        }  
    }  
}