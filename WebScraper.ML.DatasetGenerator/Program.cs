using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.WebApi.Helpers;
using WebScraper.WebApi.Models;
using WebScraper.WebApi.Models.Factories;

namespace WebScraper.ML.DatasetGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = RegisterServices().BuildServiceProvider();

            var productWatcherManager = serviceProvider.GetService<ProductWatcherManager>();
            var productDto = await productWatcherManager.GetProductAsync(1007);

            var htmlLoaderFactory = serviceProvider.GetService<HtmlLoaderFactory>();
            var htmlLoader = htmlLoaderFactory.Get(productDto.Site);

            var cancelationSource = new CancellationTokenSource();
            var document = await htmlLoader.Load(productDto.Url, productDto.Site, cancelationSource.Token);
            var htmlElements = document.QuerySelectorAll("*").Where(el => el.ChildElementCount == 0 && !String.IsNullOrEmpty(el.OuterHtml));
            htmlElements = htmlElements.OfTypes(new Type[] 
            {
                typeof(IHtmlSpanElement),
                typeof(IHtmlDivElement),
                typeof(IHtmlMetaElement),
                typeof(IHtmlListItemElement)
            });

            var datasetGeneratorSettings = serviceProvider.GetService<IConfiguration>().GetSection(productDto.Site.Name).Get<DatasetGeneratorSettings>();
            var dic = new Dictionary<string, HtmlDataSet>();
            foreach (var element in htmlElements)
            {
                bool isContainsPrice = false;                

                string htmlElement = element.OuterHtml.Replace("\n","").Replace("\r\n", "");

                if (!dic.ContainsKey(htmlElement))
                {
                    foreach (var priceTag in datasetGeneratorSettings.PriceTags)
                        if (element.OuterHtml.Contains(priceTag))
                            isContainsPrice = true;

                    dic.Add(htmlElement, new HtmlDataSet { IsContainsPrice = isContainsPrice, HtmlElement = htmlElement });
                }
            }

            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
            csvConfiguration.Delimiter = ",";
            csvConfiguration.ShouldQuote = (a, b) => true;
            csvConfiguration.HasHeaderRecord = true;
            csvConfiguration.TypeConverterCache.AddConverter<bool>(new BinaryBooleanConverter());

            using StreamWriter streamWriter = new StreamWriter("test.csv");
            using CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration);

            csvWriter.WriteHeader<HtmlDataSet>();
            csvWriter.NextRecord();
            csvWriter.WriteRecords(dic.Values.ToList());
        }

        static IServiceCollection RegisterServices()
        {
            var builder = new ConfigurationBuilder()
                 .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                 .AddJsonFile($"parserSettings.json", optional: false, reloadOnChange: true)
                 .AddJsonFile($"datasetGeneratorSettings.json", optional: false, reloadOnChange: true);

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
                .AddTransient<HtmlLoader>()
                .AddSingleton<SelenuimService>()
                .AddTransient<HtmlLoaderFactory>()
                .AddTransient<ProductWatcherManager>();
        }
    }
}
