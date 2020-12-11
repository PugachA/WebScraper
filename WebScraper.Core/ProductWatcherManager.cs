using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Data.Models;
using WebScraper.Data;
using WebScraper.Core.Factories;
using WebScraper.Core.Helpers;
using WebScraper.Core.Loaders;
using WebScraper.Core.DTO;
using WebScraper.Core.Cron;
using Microsoft.Extensions.Configuration;
using WebScraper.Core.Parsers;
using AngleSharp.Dom;
using System.IO;

namespace WebScraper.Core
{
    /// <summary>
    /// Фасад
    /// </summary>
    public class ProductWatcherManager
    {
        private readonly ProductWatcherContext _productWatcherContext;
        private readonly ILogger<ProductWatcherManager> _logger;
        private readonly HangfireSchedulerClient _hangfireSchedulerClient;
        private readonly PriceParserFactory _priceParserFactory;
        private readonly ScreenshotLoaderFactory _screenshotLoaderFactory;
        private readonly HtmlLoaderFactory _htmlLoaderFactory;
        private readonly IConfiguration _configuration;

        public ProductWatcherManager(ProductWatcherContext productWatcherContext,
                                     HangfireSchedulerClient hangfireSchedulerClient,
                                     PriceParserFactory priceParserFactory,
                                     ScreenshotLoaderFactory screenshotLoaderFactory,
                                     HtmlLoaderFactory htmlLoaderFactory,
                                     ILogger<ProductWatcherManager> logger,
                                     IConfiguration configuration)
        {
            _productWatcherContext = productWatcherContext;
            _hangfireSchedulerClient = hangfireSchedulerClient;
            _priceParserFactory = priceParserFactory;
            _htmlLoaderFactory = htmlLoaderFactory;
            _logger = logger;
            _configuration = configuration;
            _screenshotLoaderFactory = screenshotLoaderFactory;
        }

        public async Task<Price> ExtractPriceDto(Product product)
        {
            if (product == null)
                throw new ArgumentNullException($"Параметр {nameof(product)} не может быть null");

            var priceInfo = await ExtractPriceInfo(product);

            if (priceInfo == null)
                throw new NullReferenceException($"Не удалось извлечь {nameof(PriceInfo)} для {nameof(product)}={product}");

            var priceDto = priceInfo.ConvertToPriceDto(product.Id);

            _productWatcherContext.Prices.Add(priceDto);
            _productWatcherContext.SaveChanges();

            return priceDto;
        }

        private async Task<PriceInfo> ExtractPriceInfo(Product product)
        {
            IHtmlLoader htmlLoader = _htmlLoaderFactory.Get(product.Site);

            var cancelationSource = new CancellationTokenSource();
            var document = await htmlLoader.LoadHtml(product.Url, product.Site, cancelationSource.Token);

            var priceParser = _priceParserFactory.Get<IDocument>(product.Site);
            var parserSettings = _configuration.GetSection(product.Site.Name).Get<ParserSettings>();

            if (parserSettings == null)
                throw new ArgumentException($"Не удалось найти настройки {nameof(ParserSettings)} в конфигурации для сайта {product.Site.Name}");

            return await priceParser.Parse(document, parserSettings);
        }

        public async Task<Product> GetProductAsync(int productId)
        {
            var Product = await _productWatcherContext.Products
                .Include(p => p.Site)
                .Include(p => p.Site.Settings)
                .SingleOrDefaultAsync(p => p.Id == productId);

            return Product;
        }

        public async Task<Price> GetLastPriceDto(int productId)
        {
            var priceDto = await _productWatcherContext.Prices
                .Where(p => p.ProductId == productId)
                .OrderBy(p => p.Date)
                .LastOrDefaultAsync();

            return priceDto;
        }

        public async Task<Site> GetSite(int siteId)
        {
            return await _productWatcherContext.Sites
                .Include(s => s.Settings)
                .SingleOrDefaultAsync(s => s.Id == siteId);
        }

        public async Task<Site> GetSiteByProductUrl(Uri productUrl)
        {
            var sitesDto = await _productWatcherContext.Sites
                .Include(s => s.Settings)
                .ToListAsync();

            return sitesDto.SingleOrDefault(s => (new Uri(s.BaseUrl)).Host == productUrl.Host);
        }

        public async Task<Product> CreateProduct(string productUrl, Site siteDto, List<string> scheduler, bool pushToHangfire)
        {
            var Product = new Product(productUrl, siteDto, scheduler);

            await _productWatcherContext.Products.AddAsync(Product);

            await _productWatcherContext.SaveChangesAsync();

            if (pushToHangfire)
                await _hangfireSchedulerClient.CreateOrUpdateScheduler(
                    new ProductSchedulerDto
                    {
                        ProductId = Product.Id,
                        Scheduler = Product.Scheduler
                    });

            return Product;
        }

        public async Task<Product> UpdateProductAutogenerateScheduler(string productUrl, Site siteDto)
        {
            var Product = await this.CreateProduct(productUrl, siteDto, new List<string>(), false);

            await UpdateSiteScheduler(siteDto);

            return Product;
        }

        public async Task UpdateSiteScheduler(Site siteDto)
        {
            var products = _productWatcherContext.Products
                .Include(p => p.Site)
                .Where(p => p.Site.Id == siteDto.Id && !p.IsDeleted);

            var cronSchedulerGenerator = new CronSchedulerGenerator(siteDto.Settings);
            var productSchedulers = cronSchedulerGenerator.GenerateSchedule(products);

            foreach (var productScheduler in productSchedulers)
                await UpdateProductScheduler(productScheduler.Key, productScheduler.Value);

            _logger.LogInformation($"Обновлено расписание для сайта {nameof(siteDto.Id)}={siteDto.Id}");
        }

        public async Task UpdateProductScheduler(Product Product, List<string> scheduler)
        {
            await _hangfireSchedulerClient.DeleteProductScheduler(Product.Id);

            await _hangfireSchedulerClient.CreateOrUpdateScheduler(
                new ProductSchedulerDto
                {
                    ProductId = Product.Id,
                    Scheduler = scheduler
                });

            Product.Scheduler = scheduler;
            await _productWatcherContext.SaveChangesAsync();
        }

        public async Task SmartDelete(Product Product)
        {
            if (Product == null)
                throw new ArgumentNullException($"{nameof(Product)} не может быть null");

            Product.IsDeleted = true;
            await _productWatcherContext.SaveChangesAsync();

            await _hangfireSchedulerClient.DeleteProductScheduler(Product.Id);

            if (Product.Site.Settings.AutoGenerateSchedule)
                await UpdateSiteScheduler(Product.Site);
        }

        public async Task<PriceInfo> GetCVPriceInfo(string productUri)
        {
            var imagePath = Path.Combine(_configuration.GetValue<string>("ImagesFolder"), $"{DateTime.Now.ToString("dd-MM-yyyyTHH-mm-ss.ffffff")}.png");
            var site = new Site { Settings = new SiteSettings { HtmlLoader = "PuppeteerLoader", PriceParser = "ComputerVisionParser" } };

            var loader = _screenshotLoaderFactory.Get(site);
            var token = new CancellationToken();
            await loader.LoadScreenshot(imagePath, productUri, site, token);

            var cvPriceParser = _priceParserFactory.Get<string>(site);
            var priceInfo = await cvPriceParser.Parse(imagePath, null);

            File.Delete(imagePath);

            return priceInfo;
        }
    }
}
