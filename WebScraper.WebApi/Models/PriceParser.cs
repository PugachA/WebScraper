﻿using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Models
{
    public class PriceParser : IPriceParser
    {
        private readonly ILogger _logger;
        private readonly ParserSettings _parserSettings;

        public PriceParser(ParserSettings parserSettings, ILogger logger)
        {
            _parserSettings = parserSettings;
            _logger = logger;
        }

        public PriceInfo Parse(IHtmlDocument htmlDocument)
        {
            if (htmlDocument.Title == "Ой!")
            {
                _logger.LogError($"Попали на капчу {htmlDocument.Source.Text}");
                throw new ArgumentException($"Попали на капчу { htmlDocument.Source.Text }");
            }

            var discountPriceElement = htmlDocument.QuerySelectorAll(_parserSettings.DiscountHtmlPath).FirstOrDefault();
            _logger.LogInformation($"Обработываемая часть документа по скидке {discountPriceElement?.OuterHtml}");

            var discountPrice = discountPriceElement?.TextContent;

            var priceElement = htmlDocument.QuerySelectorAll(_parserSettings.PriceHtmlPath).FirstOrDefault();
            _logger.LogInformation($"Обработываемая часть документа по цене {priceElement?.OuterHtml}");

            var price = priceElement?.TextContent;

            if (discountPrice == null && price == null)
            {
                var info = htmlDocument.QuerySelectorAll(_parserSettings.OutOfStockHtmlPath).FirstOrDefault()?.TextContent;

                if (info != null)
                {
                    _logger.LogInformation($"Возможно нет в продаже или он удален. Info: {info}");
                    return new PriceInfo { AdditionalInformation = info };
                }

                throw new FormatException($"Неизвестная ошибка {htmlDocument.Source.Text}");
            }

            if (discountPrice != null && price == null)
            {
                price = discountPrice;
                discountPrice = null;
            }

            discountPrice = discountPrice?.Replace(" ", String.Empty).Replace("\u00A0", String.Empty);
            price = price?.Replace(" ", String.Empty).Replace("\u00A0", String.Empty);

            Regex regex = new Regex(@"\d+[.,]?\d{1,2}");
            if (discountPrice != null)
                discountPrice = regex.Match(discountPrice).Value;

            if (price != null)
                price = regex.Match(price).Value;

            if (!Decimal.TryParse(price, out decimal priceValue))
                throw new InvalidCastException($"Не удалось привести {nameof(price)}={price} к int");

            if (!Decimal.TryParse(discountPrice, out decimal discountPriceTemp) && discountPrice != null)
                throw new InvalidCastException($"Не удалось привести {nameof(discountPrice)}={discountPrice} к int");

            decimal? discountPriceValue = discountPrice == null ? null : (decimal?)discountPriceTemp;

            return new PriceInfo(priceValue, discountPriceValue, ExtractAdditionalInformation(htmlDocument));
        }

        protected string ExtractAdditionalInformation(IHtmlDocument htmlDocument)
        {
            if (_parserSettings.AdditionalInformation == null)
                return null;

            var additionaInformation = new Dictionary<string, string>();
            foreach(var keyValue in _parserSettings.AdditionalInformation)
            {
                var element = htmlDocument.QuerySelectorAll(keyValue.Value).FirstOrDefault();

                if (element == null)
                    _logger.LogWarning($"Не удалось извлечь информацию о {keyValue.Key} по пути {keyValue.Value}");

                additionaInformation.Add(keyValue.Key, element?.TextContent);
            }

            _logger.LogInformation($"Найденная дополнительная информация {JsonSerializer.Serialize(additionaInformation)}");

            return JsonSerializer.Serialize(additionaInformation);
        }
    }
}
