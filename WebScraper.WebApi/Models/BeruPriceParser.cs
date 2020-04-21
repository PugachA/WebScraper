using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Models
{
    public class BeruPriceParser : IPriceParser
    {
        private readonly ILogger _logger;

        public BeruPriceParser(ILogger logger)
        {
            _logger = logger;
        }

        public PriceInfo Parse(IHtmlDocument htmlDocument)
        {
            var discountPrice = htmlDocument.QuerySelectorAll("div._1u3j_pk1db").FirstOrDefault()?.TextContent;
            var price = htmlDocument.QuerySelectorAll("div._1vKgTDo6wh").FirstOrDefault()?.TextContent;

            if (discountPrice == null && price == null)
            {
                var info = htmlDocument.QuerySelectorAll("div._1w2bHIINJu").FirstOrDefault()?.TextContent;

                if (info != null)
                    _logger.LogInformation($"Возможно товар разобрали или он удален. Info: {info}");

                return null;
            }

            if (discountPrice != null && price == null)
            {
                price = discountPrice;
                discountPrice = null;
            }

            discountPrice = discountPrice?.Replace("₽", "").Trim();
            price = price?.Replace("₽", "").Trim();

            if (!Decimal.TryParse(price, out decimal priceValue))
                throw new InvalidCastException($"Не удалось привести {nameof(price)}={price} к int");

            if (!Decimal.TryParse(discountPrice, out decimal discountPriceTemp) && discountPrice != null)
                throw new InvalidCastException($"Не удалось привести {nameof(discountPrice)}={discountPrice} к int");

            decimal? discountPriceValue = discountPrice == null ? null : (decimal?)discountPriceTemp;

            return new PriceInfo(priceValue, discountPriceValue);
        }
    }
}
