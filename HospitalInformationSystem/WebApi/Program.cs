using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceProject.Seeder;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();
                //using (var serviceScope = host.Services.CreateScope())
                //{
                //    var dbContext = serviceScope.ServiceProvider.GetRequiredService<HISContext>();
                //    dbContext.Database.Migrate();
                //}
                using (var serviceScope = host.Services.CreateScope())
                {
                    var dataSeeder = serviceScope.ServiceProvider.GetService<TechnicianSeeder>();
                    dataSeeder.InitializeAsync(serviceScope.ServiceProvider).Wait();
                }
                host.Run();
            }
            catch (System.Exception ex)
            {

            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
