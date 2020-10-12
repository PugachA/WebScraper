using AngleSharp.Html.Dom;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Core;
using WebScraper.Core.Factories;
using WebScraper.Core.Helpers;
using WebScraper.Core.Loaders;
using WebScraper.Core.Parsers;
using WebScraper.Data;

namespace WebScraper.ML.DatasetGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = RegisterServices().BuildServiceProvider();

            var htmlDatasets1 = await GenerateDataSet(6, serviceProvider);
            var htmlDatasets = await GenerateDataSet(1, serviceProvider);

            DataSetWriter dataSetWriter = new DataSetWriter("DataSets/test.csv");
            dataSetWriter.AppendRecords(htmlDatasets);
            dataSetWriter.AppendRecords(htmlDatasets1);
        }

        static async Task<IEnumerable<HtmlDataSet>> GenerateDataSet(int productId, ServiceProvider serviceProvider)
        {
            var productWatcherManager = serviceProvider.GetService<ProductWatcherManager>();
            var productDto = await productWatcherManager.GetProductAsync(productId);

            var htmlLoaderFactory = serviceProvider.GetService<HtmlLoaderFactory>();
            var htmlLoader = htmlLoaderFactory.Get(productDto.Site);

            var cancelationSource = new CancellationTokenSource();
            var document = await htmlLoader.Load(productDto.Url, productDto.Site, cancelationSource.Token);

            var parserSettings = serviceProvider.GetService<IConfiguration>().GetSection(productDto.Site.Name).Get<ParserSettings>();
            var dataSetGeneratorSettings = serviceProvider.GetService<IConfiguration>().GetSection(productDto.Site.Name).Get<DataSetGeneratorSettings>();
            dataSetGeneratorSettings.AddParserSettings(parserSettings);

            return ParseDocument(document, dataSetGeneratorSettings);
        }

        static void FileStorageDataSetGenerate(DataSetWriter dataSetWriter)
        {

        }

        static void HttpDataSetGenerate(DataSetWriter dataSetWriter, ServiceProvider serviceProvider)
        {

        }

        static IEnumerable<HtmlDataSet> ParseDocument(IHtmlDocument document, DataSetGeneratorSettings dataSetGeneratorSettings)
        {
            var htmlElements = document.QuerySelectorAll("*").Where(el => el.ChildElementCount == 0 && !String.IsNullOrEmpty(el.OuterHtml));
            htmlElements = htmlElements.OfTypes(new Type[]
            {
                typeof(IHtmlSpanElement),
                typeof(IHtmlDivElement),
                typeof(IHtmlMetaElement),
                typeof(IHtmlListItemElement)
            });


            var dic = new Dictionary<string, HtmlDataSet>();
            foreach (var element in htmlElements)
            {
                bool isContainsPrice = false;

                string htmlElement = element.OuterHtml.Replace("\n", "").Replace("\r\n", "");

                if (!dic.ContainsKey(htmlElement))
                {
                    foreach (var priceTag in dataSetGeneratorSettings.PriceTags)
                        if (element.OuterHtml.Contains(priceTag))
                            isContainsPrice = true;

                    dic.Add(htmlElement, new HtmlDataSet { IsContainsPrice = isContainsPrice, HtmlElement = htmlElement });
                }
            }

            return dic.Values;
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
                .AddTransient<HttpLoader>()
                .AddSingleton<SelenuimLoader>()
                .AddSingleton<PuppeteerLoader>()
                .AddTransient<HtmlLoaderFactory>()
                .AddTransient<ProductWatcherManager>();
        }
    }
}
