using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebScraper.Core;
using WebScraper.Core.Factories;
using WebScraper.Core.Helpers;
using WebScraper.Core.Loaders;
using WebScraper.Data;

namespace WebScraper.Tests
{
    public class SelenuimServiceTests
    {
        private ServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            serviceProvider = RegisterServices().BuildServiceProvider();
        }
        public IServiceCollection RegisterServices()
        {
            var builder = new ConfigurationBuilder()
                 .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                 .AddJsonFile($"parserSettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            return new ServiceCollection()
                .AddDbContext<ProductWatcherContext>(
                    options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")),
                    ServiceLifetime.Transient,
                    ServiceLifetime.Transient)
                .AddLogging(configure => configure.AddConsole())
                .AddTransient<IConfiguration>(provider => configuration)
                .AddTransient<HangfireSchedulerClient>()
                .AddTransient<PriceParserFactory>()
                .AddTransient<HttpLoader>()
                .AddSingleton<SelenuimLoader>()
                .AddTransient<HtmlLoaderFactory>()
                .AddTransient<ProductWatcherManager>();
        }

        [Test]
        public async Task RandomTest()
        {
            var productWatcherManager = serviceProvider.GetService<ProductWatcherManager>();
            var productDto = await productWatcherManager.GetProductAsync(1);
            var price = await productWatcherManager.ExtractPriceDto(productDto);
            price = await productWatcherManager.ExtractPriceDto(productDto);
            price = await productWatcherManager.ExtractPriceDto(productDto);
            price = await productWatcherManager.ExtractPriceDto(productDto);
            price = await productWatcherManager.ExtractPriceDto(productDto);
            price = await productWatcherManager.ExtractPriceDto(productDto);
            price = await productWatcherManager.ExtractPriceDto(productDto);
            price = await productWatcherManager.ExtractPriceDto(productDto);
            price = await productWatcherManager.ExtractPriceDto(productDto);
            price = await productWatcherManager.ExtractPriceDto(productDto);
            price = await productWatcherManager.ExtractPriceDto(productDto);
            price = await productWatcherManager.ExtractPriceDto(productDto);
        }
    }
}
