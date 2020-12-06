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
using WebScraper.Core.Extensions;
using AngleSharp.Dom;
using AngleSharp;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace WebScraper.ML.DatasetGenerator
{
    class Program
    {
        private const int PriceElementRepeatCount = 1;
        private static Random random = new Random();

        static async Task Main(string[] args)
        {
            //await GenerateMLDataSet();
            
            await GenerateCVImages();
        }

        static async Task GenerateCVImages()
        {
            var serviceProvider = RegisterServices().BuildServiceProvider();

            Directory.Delete("Data/Images", true);

            await foreach (var product in GetProducts(serviceProvider))
            {
                var screenshotLoaderFactory = serviceProvider.GetService<ScreenshotLoaderFactory>();
                var screenshotLoader = screenshotLoaderFactory.Get(product.Site);

                var imageDirectoryPath = $"Data/Images/{product.Site.Name}";
                if (!Directory.Exists(imageDirectoryPath))
                    Directory.CreateDirectory(imageDirectoryPath);

                var cancelationSource = new CancellationTokenSource();
                await screenshotLoader.LoadScreenshot($"{imageDirectoryPath}/{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.png", product.Url, product.Site, cancelationSource.Token);
            }
        }

        static async Task GenerateMLDataSet()
        {
            var serviceProvider = RegisterServices().BuildServiceProvider();

            DataSetWriter dataSetWriter = new DataSetWriter();

            await foreach (var product in GetProducts(serviceProvider))
                await dataSetWriter.AppendRecordsAsync(await HttpDataSetGenerate(product, serviceProvider));

            foreach (var folderPath in Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "Data/HtmlFiles")))
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
            var document = await htmlLoader.LoadHtml(product.Url, product.Site, cancelationSource.Token);

            var parserSettings = serviceProvider.GetService<IConfiguration>().GetSection(product.Site.Name).Get<ParserSettings>();
            var dataSetGeneratorSettings = serviceProvider.GetService<IConfiguration>().GetSection(product.Site.Name).Get<DataSetGeneratorSettings>();
            dataSetGeneratorSettings.AddParserSettings(parserSettings);

            return ParseDocument(document, dataSetGeneratorSettings, PriceElementRepeatCount);
        }

        static async Task<IEnumerable<HtmlDataSet>> FileStorageDataSetGenerate(string folderPath, ServiceProvider serviceProvider, DataSetGeneratorSettings dataSetSettings)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();

            List<HtmlDataSet> list = new List<HtmlDataSet>();
            foreach (var filePath in Directory.GetFiles(folderPath, "*.txt"))
            {
                var document = parser.ParseDocument(await File.ReadAllTextAsync(filePath));
                list.AddRange(ParseDocument(document, dataSetSettings, PriceElementRepeatCount));
            }

            return list;
        }

        static IEnumerable<HtmlDataSet> ParseDocument(IDocument document, DataSetGeneratorSettings dataSetGeneratorSettings, int priceElementRepeatCount)
        {
            var htmlElements = document.QuerySelectorAll("*").Where(el => el.ChildElementCount == 0 && !String.IsNullOrEmpty(el.OuterHtml));
            htmlElements = htmlElements.OfTypes(new Type[]
            {
                typeof(IHtmlSpanElement),
                typeof(IHtmlDivElement),
                typeof(IHtmlMetaElement),
                typeof(IHtmlListItemElement)
            });

            var list = new List<HtmlDataSet>();
            foreach (var element in htmlElements)
            {
                bool isContainsPrice = false;

                foreach (var priceTag in dataSetGeneratorSettings.PriceTags)
                    if (element.OuterHtml.Contains(priceTag))
                        isContainsPrice = true;

                foreach (var regex in dataSetGeneratorSettings.Regex)
                    if (Regex.IsMatch(element.OuterHtml, regex))
                        isContainsPrice = true;

                var htmlElement = Transform(element.OuterHtml);

                if (isContainsPrice)
                {
                    for (int i = 0; i < priceElementRepeatCount; i++)
                    {
                        var randomGeneratedHtmlElement = GenerateRandomPriceElement(htmlElement);
                        list.Add(new HtmlDataSet { IsContainsPrice = isContainsPrice, HtmlElement = randomGeneratedHtmlElement, ClassName = element.ClassName, HtmlElementName = element.LocalName });
                    }
                }

                list.Add(new HtmlDataSet { IsContainsPrice = isContainsPrice, HtmlElement = htmlElement, ClassName = element.ClassName, HtmlElementName=element.LocalName });
            }

            return list;
        }

        static string Transform(string textContent)
        {
            return textContent.Replace("\n", "").Replace("\r\n", "").Replace("&nbsp;", " ").Replace("<!-- -->", "");
        }

        static string GenerateRandomPriceElement(string element)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var priceDocument = context.OpenAsync(req => req.Content(element)).Result;

            var newTextContent = GerenateRandomTextContent(priceDocument.DocumentElement.TextContent);
            priceDocument.DocumentElement.LastElementChild.LastElementChild.TextContent = newTextContent;

            return priceDocument.DocumentElement.LastElementChild.InnerHtml;
        }

        static string GerenateRandomTextContent(string textContent)
        {
            foreach (Match m in Regex.Matches(textContent, @"\d"))
                textContent = textContent.Replace(m.Value, random.Next(0, 9).ToString());

            return textContent;
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
                .AddTransient<ScreenshotLoaderFactory>()
                .AddTransient<ProductWatcherManager>();
        }

        static async IAsyncEnumerable<Product> GetProducts(ServiceProvider serviceProvider)
        {
            var productWatcherContext = serviceProvider.GetService<ProductWatcherContext>();

            await foreach (var product in productWatcherContext.Products
                .Include(p => p.Site)
                .Include(p => p.Site.Settings)
                .AsAsyncEnumerable()
                .Where(p => p.IsDeleted == false && (p.Site.Name != "Letual" && p.Site.Name != "Youla" && p.Site.Name != "Onlinetrade") && p.Site.Settings.HtmlLoader != "HttpLoader"))
                yield return product;
        }

        static async IAsyncEnumerable<Product> GetCVProducts(ServiceProvider serviceProvider)
        {
            var productWatcherContext = serviceProvider.GetService<ProductWatcherContext>();

            await foreach (var product in productWatcherContext.Products
                .Include(p => p.Site)
                .Include(p => p.Site.Settings)
                .AsAsyncEnumerable()
                .Where(p => p.IsDeleted == false && p.Site.Settings.HtmlLoader != "HttpLoader"))
                yield return product;
        }
    }
}
