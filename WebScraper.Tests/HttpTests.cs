using AngleSharp;
using AngleSharp.Html.Parser;
using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraper.WebApi.DTO;
using WebScraper.WebApi.Models;
using WebScraper.WebApi.Models.Factories;

namespace WebScraper.Tests
{
    public class HttpTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task BeruTest()
        {
            var site = new SiteDto("Beru", null);
            var product = new ProductDto(
                @"https://beru.ru/product/finish-opolaskivatel-dlia-posudomoechnoi-mashiny-0-4-l/100235939298?show-uid=15827428683748816103006024",
                site,
                null);

            var htmlLoader = new HtmlLoader(new Uri(product.Url));
            var document = await htmlLoader.Load();

            var parserFactory = new PriceParserFactory();
            var priceParser = parserFactory.Get(product.Site);

            var priceInfo = priceParser.Parse(document);
        }

        [Test]
        public async Task RegardTest()
        {
            var client = new RestClient("https://www.regard.ru/catalog/tovar299743.htm");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            string source = response.Content;
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(source);

            var price = document.QuerySelectorAll("span").SingleOrDefault(i => i.ClassName != null && i.ClassName == "price lot");
        }
    }
}
