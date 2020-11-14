using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Core;
using WebScraper.Core.Factories;
using WebScraper.Core.Helpers;
using WebScraper.Core.Loaders;
using WebScraper.Core.Parsers;
using WebScraper.Data;
using WebScraper.Data.Models;

namespace WebScraper.ML.DatasetGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = RegisterServices().BuildServiceProvider();

            DataSetWriter dataSetWriter = new DataSetWriter("DataSets/test.csv");

            await foreach (var product in GetProducts(serviceProvider))
                await dataSetWriter.AppendRecordsAsync(await HttpDataSetGenerate(product, serviceProvider));

            foreach (var folderPath in Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "HtmlFiles")))
            {
                var siteName = folderPath.Split(Path.DirectorySeparatorChar).Last();
                var parserSettings = serviceProvider.GetService<IConfiguration>().GetSection(siteName).Get<ParserSettings>();
                var dataSetGeneratorSettings = serviceProvider.GetService<IConfiguration>().GetSection(siteName).Get<DataSetGeneratorSettings>();
                dataSetGeneratorSettings.AddParserSettings(parserSettings);

                await dataSetWriter.AppendRecordsAsync(await FileStorageDataSetGenerate(folderPath, serviceProvider, dataSetGeneratorSettings));
            }
        }

        static async Task<IEnumerable<HtmlDataSet>> HttpDataSetGenerate(Product product, ServiceProvider serviceProvider)
        {
            var htmlLoaderFactory = serviceProvider.GetService<HtmlLoaderFactory>();
            var htmlLoader = htmlLoaderFactory.Get(product.Site);

            var cancelationSource = new CancellationTokenSource();
            var document = await htmlLoader.Load(product.Url, product.Site, cancelationSource.Token);

            var parserSettings = serviceProvider.GetService<IConfiguration>().GetSection(product.Site.Name).Get<ParserSettings>();
            var dataSetGeneratorSettings = serviceProvider.GetService<IConfiguration>().GetSection(product.Site.Name).Get<DataSetGeneratorSettings>();
            dataSetGeneratorSettings.AddParserSettings(parserSettings);

            return ParseDocument(document, dataSetGeneratorSettings);
        }

        static async Task<IEnumerable<HtmlDataSet>> FileStorageDataSetGenerate(string folderPath, ServiceProvider serviceProvider, DataSetGeneratorSettings dataSetSettings)
        {
            var context = AngleSharp.BrowsingContext.New(AngleSharp.Configuration.Default);
            var parser = context.GetService<IHtmlParser>();

            List<HtmlDataSet> list = new List<HtmlDataSet>();
            foreach (var filePath in Directory.GetFiles(folderPath, "*.txt"))
            {
                var document = parser.ParseDocument(await File.ReadAllTextAsync(filePath));
                list.AddRange(ParseDocument(document, dataSetSettings));
            }

            return list;
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

                    foreach (var regex in dataSetGeneratorSettings.Regex)
                        if (Regex.IsMatch(element.OuterHtml, regex))
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
                .AddSingleton<HeadlessPuppeteerLoader>()
                .AddTransient<HtmlLoaderFactory>()
                .AddTransient<ProductWatcherManager>();
        }

        static async IAsyncEnumerable<Product> GetProducts(ServiceProvider serviceProvider)
        {
            var productWatcherContext = serviceProvider.GetService<ProductWatcherContext>();

            await foreach (var product in productWatcherContext.Products
                .Include(p => p.Site)
                .Include(p => p.Site.Settings)
                .AsAsyncEnumerable()
                .Where(p => p.IsDeleted == false && (p.Site.Name != "Letual" && p.Site.Name != "Youla")))
                yield return product;
        }
    }
}
