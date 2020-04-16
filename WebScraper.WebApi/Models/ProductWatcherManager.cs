using Microsoft.EntityFrameworkCore;
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

        public ProductWatcherManager(ProductWatcherContext productWatcherContext)
        {
            _productWatcherContext = productWatcherContext;
        }

        public async Task ExtractPriceDto(int productId)
        {
            var priceInfo = await ExtractPriceInfo(productId);

            if (priceInfo == null)
                throw new NullReferenceException($"Не удалось извлечь {nameof(PriceInfo)} для {nameof(productId)}={productId}");

            var priceDto = ConvertToPriceDto(priceInfo, productId);

            _productWatcherContext.Prices.Add(priceDto);
            _productWatcherContext.SaveChanges();
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

        private async Task<PriceInfo> ExtractPriceInfo(int productId)
        {
            var product = await GetProductAsync(productId);

            if (product == null)
                throw new NullReferenceException($"Не удалось найти продукт по {nameof(productId)}={productId}");

            var htmlLoader = new HtmlLoader(new Uri(product.Url));
            var document = await htmlLoader.Load();

            var parserFactory = new PriceParserFactory();
            var priceParser = parserFactory.Get(product.Site);

            var priceInfo = priceParser.Parse(document);

            return priceInfo;
        }

        private async Task<ProductDto> GetProductAsync(int productId)
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
