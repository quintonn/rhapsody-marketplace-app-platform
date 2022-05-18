using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Marketplace
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = new WebHostBuilder();

            //builder.UseIISIntegration();
            builder.UseKestrel();


            builder.UseContentRoot(Directory.GetCurrentDirectory());
            var config = new ConfigurationBuilder();
            config.AddJsonFile("appsettings.json", true, true);
            builder.UseConfiguration(config.Build());

            builder.ConfigureLogging(x =>
            {
                x.SetMinimumLevel(LogLevel.Information);

                x.ClearProviders();
                x.AddDebug();
                x.AddConsole();
            });
            builder.UseStartup<Startup>();

            return builder;
        }
    }
}
