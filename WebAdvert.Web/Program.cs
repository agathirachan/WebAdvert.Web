using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAdvert.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //To read configuration from parameter store for docker
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var envName = hostingContext.HostingEnvironment.EnvironmentName.ToString().ToLower();
                    config.AddSystemsManager($"/dockerwa");
                })
                //.ConfigureAppConfiguration((hostingContext, config) =>
                //{
                //    var envName = hostingContext.HostingEnvironment.EnvironmentName.ToString().ToLower();
                //    config.AddSystemsManager($"/{envName}", TimeSpan.FromMinutes(5));
                //})
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
