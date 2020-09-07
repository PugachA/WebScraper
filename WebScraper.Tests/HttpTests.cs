using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace WebScraper.Tests
{
    public class HttpTests
    {
        private Mock<ILogger>  mockLogger; 

        [SetUp]
        public void Setup()
        {
            mockLogger = new Mock<ILogger>();
        }

        //[Test]
        //public async Task BeruTest()
        //{
        //    var site = new SiteDto("Beru", null);
        //    var product = new ProductDto(
        //        @"https://beru.ru/product/frima-zamenitel-sukhogo-molochnogo-produkta-500-g/100714261945?offerid=yuVX7VkKijDxDwzZnPR6yw&track=cart",
        //        site,
        //        null);

        //    var htmlLoader = new HtmlLoader(mockLogger.Object);
        //    var document = await htmlLoader.Load(product.Url);

        //    var parserFactory = new PriceParserFactory(mockLogger.Object);
        //    var priceParser = parserFactory.Get(product.Site);

        //    var priceInfo = priceParser.Parse(document);
        //}

        //[Test]
        //public async Task RegardTest()
        //{
        //    var site = new SiteDto("Ozon", null);
        //    var product = new ProductDto(
        //        @"https://www.regard.ru/catalog/tovar299743.htm",
        //        site,
        //        null);

        //    var htmlLoader = new HtmlLoader(mockLogger.Object);
        //    var document = await htmlLoader.Load(product.Url);

        //    var price = document.QuerySelectorAll("span").SingleOrDefault(i => i.ClassName != null && i.ClassName == "price lot");
        //}

        //[Test]
        //public async Task OzonTest()
        //{
        //    var site = new SiteDto("Ozon", null);
        //    var product = new ProductDto(
        //        @"https://www.ozon.ru/context/detail/id/147947696/",
        //        site,
        //        null);

        //    var htmlLoader = new HtmlLoader(mockLogger.Object);
        //    var document = await htmlLoader.Load(product.Url);
        //}

    }
}
