using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Models
{
    public class BeruPriceParser : IPriceParser
    {
        public PriceInfoDto Parse(IHtmlDocument htmlDocument)
        {
            var discountPrice = htmlDocument.QuerySelectorAll("div._1u3j_pk1db").FirstOrDefault().TextContent;
            discountPrice = discountPrice.Replace("₽", "").Trim();

            var price = htmlDocument.QuerySelectorAll("div._1vKgTDo6wh").FirstOrDefault().TextContent;
            price = price.Replace("₽", "").Trim();

            if (discountPrice != null && price == null)
            {
                price = discountPrice;
                discountPrice = null;
            }

            if (!int.TryParse(price, out int priceValue))
                throw new InvalidCastException();

            if (!int.TryParse(discountPrice, out int discountPriceTemp) && discountPrice != null)
                throw new InvalidCastException();

            int? discountPriceValue = discountPrice == null ? null : (int?)discountPriceTemp * 100;

            return new PriceInfoDto(priceValue * 100, discountPriceValue);
        }
    }
}
