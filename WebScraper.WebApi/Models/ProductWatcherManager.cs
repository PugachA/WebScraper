using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.WebApi.Cron;
using WebScraper.WebApi.DTO;
using WebScraper.WebApi.Helpers;
using WebScraper.WebApi.Models.Factories;

namespace WebScraper.WebApi.Models
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

        public async Task<PriceDto> ExtractPriceDto(ProductDto product)
        {
            if (product == null)
                throw new ArgumentNullException($"Параметр {nameof(product)} не может быть null");

            var priceInfo = await ExtractPriceInfo(product);

            if (priceInfo == null)
                throw new NullReferenceException($"Не удалось извлечь {nameof(PriceInfo)} для {nameof(product)}={product}");

            var priceDto = ConvertToPriceDto(priceInfo, product.Id);

            _productWatcherContext.Prices.Add(priceDto);
            _productWatcherContext.SaveChanges();

            return priceDto;
        }

        private PriceDto ConvertToPriceDto(PriceInfo priceInfo, int productId)
        {
            if (priceInfo == null)
                throw new ArgumentNullException($"Параметр {nameof(priceInfo)} не может быть null");

            var priceDto = new PriceDto
            {
                Price = priceInfo.Price,
                DicountPrice = priceInfo.DicountPrice,
                DiscountPercentage = priceInfo.DiscountPercentage,
                AdditionalInformation = priceInfo.AdditionalInformation,
                Date = DateTime.Now,
                ProductId = productId
            };

            return priceDto;
        }

        private async Task<PriceInfo> ExtractPriceInfo(ProductDto product)
        {
            IHtmlLoader htmlLoader = _htmlLoaderFactory.Get(product.Site);

            var cancelationSource = new CancellationTokenSource();
            var document = await htmlLoader.Load(product.Url, cancelationSource.Token);

            var priceParser = _priceParserFactory.Get(product.Site);

            var priceInfo = priceParser.Parse(document);

            return priceInfo;
        }

        public async Task<ProductDto> GetProductAsync(int productId)
        {
            var productDto = await _productWatcherContext.Products
                .Include(p => p.Site)
                .Include(p => p.Site.Settings)
                .SingleOrDefaultAsync(p => p.Id == productId);

            return productDto;
        }

        public async Task<PriceDto> GetLastPriceDto(int productId)
        {
            var priceDto = await _productWatcherContext.Prices
                .Where(p => p.ProductId == productId)
                .OrderBy(p => p.Date)
                .LastOrDefaultAsync();

            return priceDto;
        }

        public async Task<SiteDto> GetSite(int siteId)
        {
            return await _productWatcherContext.Sites
                .Include(s => s.Settings)
                .SingleOrDefaultAsync(s => s.Id == siteId);
        }

        public async Task<SiteDto> GetSiteByProductUrl(Uri productUrl)
        {
            var sitesDto = await _productWatcherContext.Sites
                .Include(s => s.Settings)
                .ToListAsync();

            return sitesDto.SingleOrDefault(s => (new Uri(s.BaseUrl)).Host == productUrl.Host);
        }

        public async Task<ProductDto> CreateProduct(string productUrl, SiteDto siteDto, List<string> scheduler, bool pushToHangfire)
        {
            var productDto = new ProductDto(productUrl, siteDto, scheduler);

            await _productWatcherContext.Products.AddAsync(productDto);

            await _productWatcherContext.SaveChangesAsync();

            if (pushToHangfire)
                await _hangfireSchedulerClient.CreateOrUpdateScheduler(
                    new ProductSchedulerDto
                    {
                        ProductId = productDto.Id,
                        Scheduler = productDto.Scheduler
                    });

            return productDto;
        }

        public async Task<ProductDto> UpdateProductAutogenerateScheduler(string productUrl, SiteDto siteDto)
        {
            var productDto = await this.CreateProduct(productUrl, siteDto, new List<string>(), false);

            await UpdateSiteScheduler(siteDto);

            return productDto;
        }

        public async Task UpdateSiteScheduler(SiteDto siteDto)
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

        public async Task UpdateProductScheduler(ProductDto productDto, List<string> scheduler)
        {
            await _hangfireSchedulerClient.DeleteProductScheduler(productDto.Id);

            await _hangfireSchedulerClient.CreateOrUpdateScheduler(
                new ProductSchedulerDto
                {
                    ProductId = productDto.Id,
                    Scheduler = scheduler
                });

            productDto.Scheduler = scheduler;
            await _productWatcherContext.SaveChangesAsync();
        }

        public async Task SmartDelete(ProductDto productDto)
        {
            if (productDto == null)
                throw new ArgumentNullException($"{nameof(productDto)} не может быть null");

            await _hangfireSchedulerClient.DeleteProductScheduler(productDto.Id);

            productDto.IsDeleted = true;
            await _productWatcherContext.SaveChangesAsync();

            if (productDto.Site.Settings.AutoGenerateSchedule)
                await UpdateSiteScheduler(productDto.Site);
        }
    }
}
