using AngleSharp;
using AngleSharp.Html.Parser;
using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraper.WebApi.Models;

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
            HtmlLoader htmlLoader = new HtmlLoader(new Uri(@"https://beru.ru/product/finish-opolaskivatel-dlia-posudomoechnoi-mashiny-0-4-l/100235939298?show-uid=15827428683748816103006024"));
            var document = await htmlLoader.Load();

            var discountPrice = document.QuerySelectorAll("div._1u3j_pk1db").FirstOrDefault().TextContent;
            discountPrice = discountPrice.Replace("₽", "").Trim();

            var price = document.QuerySelectorAll("div._1vKgTDo6wh").FirstOrDefault().TextContent;
            price = price.Replace("₽", "").Trim();

            if (discountPrice != null && price == null)
            {
                price = discountPrice;
                discountPrice = null;
            }
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
