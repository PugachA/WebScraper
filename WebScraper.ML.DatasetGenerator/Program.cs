using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using WebScraper.WebApi.Models;
using WebScraper.WebApi.Models.Factories;

namespace WebScraper.ML.DatasetGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                 .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                 .AddJsonFile($"parserSettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            var serviceProvider = RegisterServices().BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();

        }

        static IServiceCollection RegisterServices()
        {
            var builder = new ConfigurationBuilder()
                 .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                 .AddJsonFile($"parserSettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            return new ServiceCollection()
                .AddLogging(configure => configure.AddConsole())
                .AddTransient<IConfiguration>(provider => configuration)
                .AddTransient<PriceParserFactory>()
                .AddTransient<HtmlLoader>()
                .AddSingleton<SelenuimService>()
                .AddTransient<HtmlLoaderFactory>();
        }
    }
}
