﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Marketplace
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Main starting");
            try
            {
                CreateHostBuilder(args).Build().Run();
                Console.WriteLine("Main after run");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Application error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        static long MAX_REQUEST_BODY_BYTES = 100 * 1024 * 1024; // 100MB

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = new WebHostBuilder();

            //builder.UseIISIntegration();
            builder.UseKestrel(opt =>
            {
                opt.Limits.MaxRequestBodySize = MAX_REQUEST_BODY_BYTES;
            });


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
