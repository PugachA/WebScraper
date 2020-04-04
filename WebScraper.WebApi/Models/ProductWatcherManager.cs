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
        public async Task<PriceInfo> ExtractPriceInfo(int productId)
        {
            //var site = new Site("Beru", null);
            //var product = new Product(
            //    new Uri(@"https://beru.ru/product/finish-opolaskivatel-dlia-posudomoechnoi-mashiny-0-4-l/100235939298?show-uid=15827428683748816103006024"),
            //    site,
            //    null);

            var product = await GetProductAsync(productId);

            var htmlLoader = new HtmlLoader(product.Url);
            var document = await htmlLoader.Load();

            var parserFactory = new PriceParserFactory();
            var priceParser = parserFactory.Get(product.Site);

            var priceInfo = priceParser.Parse(document);

            return priceInfo;
        }

        private async Task<Product> GetProductAsync(int productId)
        {
            return null;
        }
    }
}
