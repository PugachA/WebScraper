﻿using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            if(htmlDocument.Title == "Ой!")
            {
                _logger.LogError($"Попали на капчу {htmlDocument.Source.Text}");
                throw new ArgumentException($"Попали на капчу { htmlDocument.Source.Text }");
            }

            var discountPriceElement = htmlDocument.QuerySelectorAll("div._1u3j_pk1db").FirstOrDefault();
            _logger.LogInformation($"Обработываемая часть документа по скидке {discountPriceElement?.OuterHtml}");

            var discountPrice = discountPriceElement?.TextContent;

            var priceElement = htmlDocument.QuerySelectorAll("div._1vKgTDo6wh").FirstOrDefault();
            _logger.LogInformation($"Обработываемая часть документа по цене {priceElement?.OuterHtml}");

            var price = priceElement?.TextContent;

            if (discountPrice == null && price == null)
            {
                var info = htmlDocument.QuerySelectorAll("div._1w2bHIINJu").FirstOrDefault()?.TextContent;

                if (info != null)
                {
                    _logger.LogInformation($"Возможно товар разобрали или он удален. Info: {info}");
                    return new PriceInfo { AdditionalInformation = info };
                }

                throw new FormatException($"Неизвестная ошибка { htmlDocument.Source.Text }");
            }

            if (discountPrice != null && price == null)
            {
                price = discountPrice;
                discountPrice = null;
            }

            discountPrice = discountPrice?.Replace(" ", "");
            price = price?.Replace(" ", "");

            Regex regex = new Regex(@"\d+");
            if (discountPrice != null)
                discountPrice = regex.Match(discountPrice).Value;

            if (price != null)
                price = regex.Match(price).Value;

            if (!Decimal.TryParse(price, out decimal priceValue))
                throw new InvalidCastException($"Не удалось привести {nameof(price)}={price} к int");

            if (!Decimal.TryParse(discountPrice, out decimal discountPriceTemp) && discountPrice != null)
                throw new InvalidCastException($"Не удалось привести {nameof(discountPrice)}={discountPrice} к int");

            decimal? discountPriceValue = discountPrice == null ? null : (decimal?)discountPriceTemp;

            return new PriceInfo(priceValue, discountPriceValue);
        }
    }
}
