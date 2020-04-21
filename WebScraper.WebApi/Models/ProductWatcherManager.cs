using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebScraper.WebApi.DTO;
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

        public ProductWatcherManager(ProductWatcherContext productWatcherContext, ILogger logger)
        {
            _productWatcherContext = productWatcherContext;
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
                Date = DateTime.Now,
                ProductId = productId
            };

            return priceDto;
        }

        private async Task<PriceInfo> ExtractPriceInfo(ProductDto product)
        {
            var htmlLoader = new HtmlLoader(new Uri(product.Url));
            var document = await htmlLoader.Load();

            var parserFactory = new PriceParserFactory(_logger);
            var priceParser = parserFactory.Get(product.Site);

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
    }
}
