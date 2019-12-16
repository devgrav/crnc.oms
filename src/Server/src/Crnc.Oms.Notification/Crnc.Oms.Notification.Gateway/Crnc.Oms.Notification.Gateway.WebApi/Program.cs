using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notification.Gateway.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config)=>{
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);                    
                    config.AddEnvironmentVariables();                    
                })
                .ConfigureKestrel(options => {})
                .UseStartup<Startup>();
    }
}