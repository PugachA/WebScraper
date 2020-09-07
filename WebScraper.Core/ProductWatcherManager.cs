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

namespace WebScraper.Core
{
    /// <summary>
    /// Фасад
    /// </summary>
    public class ProductWatcherManager
    {
        private readonly ProductWatcherContext _productWatcherContext;
        private readonly ILogger _logger;
        private readonly HangfireSchedulerClient _hangfireSchedulerClient;
        private readonly PriceParserFactory _priceParserFactory;
        private readonly HtmlLoaderFactory _htmlLoaderFactory;

        public ProductWatcherManager(ProductWatcherContext productWatcherContext,
                                     HangfireSchedulerClient hangfireSchedulerClient,
                                     PriceParserFactory priceParserFactory,
                                     HtmlLoaderFactory htmlLoaderFactory,
                                     ILogger<ProductWatcherManager> logger)
        {
            _productWatcherContext = productWatcherContext;
            _hangfireSchedulerClient = hangfireSchedulerClient;
            _priceParserFactory = priceParserFactory;
            _htmlLoaderFactory = htmlLoaderFactory;
            _logger = logger;
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
            var document = await htmlLoader.Load(product.Url, product.Site, cancelationSource.Token);

            var priceParser = _priceParserFactory.Get(product.Site);

            var priceInfo = priceParser.Parse(document);

            return priceInfo;
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

            await _hangfireSchedulerClient.DeleteProductScheduler(Product.Id);

            Product.IsDeleted = true;
            await _productWatcherContext.SaveChangesAsync();

            if (Product.Site.Settings.AutoGenerateSchedule)
                await UpdateSiteScheduler(Product.Site);
        }
    }
}
