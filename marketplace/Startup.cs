using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QBic.Core.Utilities;
using Marketplace.SiteSpecific;
using System;
using WebsiteTemplate;


namespace Marketplace
{
    public class Startup
    {
        public static IConfiguration Config;

        public Startup(IConfiguration config)
        {
            Config = config;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            // This is needed if hosting on Linux environment such as azure
            services.AddLogging(x =>
            {
                x.AddConsole();
                x.AddDebug();
            });
            Console.WriteLine("*****************");
            try
            {
                var tmp = TempDataStore.GetInstance(true, true, Config, services);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting temp data store instance");
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("DONE");

            //services.AddScoped<UserInjector, DefaultUserInjector>(); // can i override the default one?
            //User can override UserInjector with their own injector class
            services.UseQBic<AppSettings, AppStartup>(Config);
        }

        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, ILoggerFactory logFactory)
        {
            // Setup internal logging system. Inherited from .net 4
            SystemLogger.Setup(logFactory);

            app.UseQBic(serviceProvider);
        }
    }
}