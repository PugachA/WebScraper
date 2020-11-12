﻿using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebScraper.Core.Parsers
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

            var nameElement = htmlDocument.QuerySelectorAll(_parserSettings.Name).FirstOrDefault();
            _logger.LogInformation($"Обработываемая часть документа по имени товара {nameElement?.OuterHtml}");

            var name = nameElement?.TextContent.Trim();

            var discountPriceElement = htmlDocument.QuerySelectorAll(_parserSettings.DiscountHtmlPath).FirstOrDefault();
            _logger.LogInformation($"Обработываемая часть документа по скидке {discountPriceElement?.OuterHtml}");

            var discountPrice = discountPriceElement?.TextContent;

            string price = default;
            foreach (var priceHtmlPath in _parserSettings.PriceHtmlPath)
            {
                var priceElement = htmlDocument.QuerySelectorAll(priceHtmlPath).FirstOrDefault();
                _logger.LogInformation($"Обработываемая часть документа по цене {priceElement?.OuterHtml}");

                price = priceElement?.TextContent;

                if (!string.IsNullOrEmpty(price))
                    break;
            }

            if (discountPrice == null && price == null)
            {
                var info = htmlDocument.QuerySelectorAll(_parserSettings.OutOfStockHtmlPath).FirstOrDefault()?.TextContent;

                if (info != null)
                {
                    _logger.LogInformation($"Возможно нет в продаже или он удален. Info: {info}");
                    return new PriceInfo { Name= name, AdditionalInformation = info };
                }

                throw new FormatException($"Неизвестная ошибка {htmlDocument.Source.Text}");
            }

            if (discountPrice != null && price == null)
            {
                price = discountPrice;
                discountPrice = null;
            }

            if (discountPrice != null)
                discountPrice = TransformPrice(discountPrice);
            if (price != null)
                price = TransformPrice(price);
             
            Regex regex = new Regex(@"\d*\.?\d{1,2}");
            if (discountPrice != null)
                discountPrice = regex.Match(discountPrice).Value;

            if (price != null)
                price = regex.Match(price).Value;

            if (!Decimal.TryParse(price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal priceValue))
                throw new InvalidCastException($"Не удалось привести {nameof(price)}={price} к int");

            if (!Decimal.TryParse(discountPrice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal discountPriceTemp) && discountPrice != null)
                throw new InvalidCastException($"Не удалось привести {nameof(discountPrice)}={discountPrice} к int");

            decimal? discountPriceValue = discountPrice == null ? null : (decimal?)discountPriceTemp;

            return new PriceInfo(priceValue, discountPriceValue, name, ExtractAdditionalInformation(htmlDocument));
        }

        protected string ExtractAdditionalInformation(IHtmlDocument htmlDocument)
        {
            if (_parserSettings.AdditionalInformation == null)
                return null;

            var additionaInformation = new Dictionary<string, string>();
            foreach (var keyValue in _parserSettings.AdditionalInformation)
            {
                var element = htmlDocument.QuerySelectorAll(keyValue.Value).FirstOrDefault();

                if (element == null)
                    _logger.LogWarning($"Не удалось извлечь информацию о {keyValue.Key} по пути {keyValue.Value}");

                var textContent = element?.TextContent ?? String.Empty;

                additionaInformation.Add(keyValue.Key, TransformAdditionalInformation(textContent));
            }

            var options = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            var additionalInformationString = JsonSerializer.Serialize(additionaInformation, options);

            _logger.LogInformation($"Найденная дополнительная информация {additionalInformationString}");

            return additionalInformationString;
        }

        private string TransformPrice(string price)
        {
            price = Regex.Replace(price, @"\s|\u00A0|\u2009", String.Empty);
            price = price.Replace(",", ".").Trim();
            return price;
        }

        private string TransformAdditionalInformation(string additionaInformation)
        {
            additionaInformation = Regex.Replace(additionaInformation, @"\u00A0|\u2009", " ");
            additionaInformation = additionaInformation.Replace("\n", "").Replace("\t", "").Trim();
            return additionaInformation;
        }
    }
}
