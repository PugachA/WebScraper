using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebScraper.Core.Parsers
{
    public class HtmlPriceParser : PriceParser<IDocument>
    {
        public HtmlPriceParser(ILogger<HtmlPriceParser> logger) : base(logger)
        { }

        public override async Task<PriceInfo> Parse(IDocument htmlDocument, ParserSettings parserSettings)
        {
            if (htmlDocument.Title == "Ой!")
            {
                logger.LogError($"Попали на капчу {htmlDocument.Source.Text}");
                throw new ArgumentException($"Попали на капчу { htmlDocument.Source.Text }");
            }

            var nameElement = htmlDocument.QuerySelectorAll(parserSettings.Name).FirstOrDefault();
            logger.LogInformation($"Обработываемая часть документа по имени товара {nameElement?.OuterHtml}");

            var name = nameElement?.TextContent.Trim();

            var discountPriceElement = htmlDocument.QuerySelectorAll(parserSettings.DiscountHtmlPath).FirstOrDefault();
            logger.LogInformation($"Обработываемая часть документа по скидке {discountPriceElement?.OuterHtml}");

            var discountPrice = discountPriceElement?.TextContent;

            string price = default;
            foreach (var priceHtmlPath in parserSettings.PriceHtmlPath)
            {
                var priceElement = htmlDocument.QuerySelectorAll(priceHtmlPath).FirstOrDefault();
                logger.LogInformation($"Обработываемая часть документа по цене {priceElement?.OuterHtml}");

                price = priceElement?.TextContent;

                if (!string.IsNullOrEmpty(price))
                    break;
            }

            if (discountPrice == null && price == null)
            {
                var info = htmlDocument.QuerySelectorAll(parserSettings.OutOfStockHtmlPath).FirstOrDefault()?.TextContent;

                if (info != null)
                {
                    logger.LogInformation($"Возможно нет в продаже или он удален. Info: {info}");
                    return new PriceInfo { Name = name, AdditionalInformation = info };
                }

                throw new FormatException($"Неизвестная ошибка {htmlDocument.Source.Text}");
            }

            if (discountPrice != null && price == null)
            {
                price = discountPrice;
                discountPrice = null;
            }

            if (discountPrice != null)
            {
                discountPrice = TransformPrice(discountPrice);
                discountPrice = ExtractPrice(discountPrice);
            }

            if (price != null)
            {
                price = TransformPrice(price);
                price = ExtractPrice(price);
            }

            if (!decimal.TryParse(price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal priceValue))
                throw new InvalidCastException($"Can not convert {nameof(price)}={price} to {typeof(decimal)}");

            if (!decimal.TryParse(discountPrice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal discountPriceTemp) && discountPrice != null)
                throw new InvalidCastException($"Can not convert {nameof(discountPrice)}={discountPrice} to {typeof(decimal)}");

            decimal? discountPriceValue = discountPrice == null ? null : (decimal?)discountPriceTemp;

            var additionalInformation = await Task.Run(() => ExtractAdditionalInformation(htmlDocument, parserSettings));

            return new PriceInfo(priceValue, discountPriceValue, name, additionalInformation);
        }

        private string ExtractAdditionalInformation(IDocument htmlDocument, ParserSettings parserSettings)
        {
            if (parserSettings.AdditionalInformation == null)
                return null;

            var additionaInformation = new Dictionary<string, string>();
            foreach (var keyValue in parserSettings.AdditionalInformation)
            {
                var element = htmlDocument.QuerySelectorAll(keyValue.Value).FirstOrDefault();

                if (element == null)
                    logger.LogWarning($"Не удалось извлечь информацию о {keyValue.Key} по пути {keyValue.Value}");

                var textContent = element?.TextContent ?? String.Empty;

                additionaInformation.Add(keyValue.Key, TransformAdditionalInformation(textContent));
            }

            var options = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            var additionalInformationString = JsonSerializer.Serialize(additionaInformation, options);

            logger.LogInformation($"Найденная дополнительная информация {additionalInformationString}");

            return additionalInformationString;
        }

        private string TransformAdditionalInformation(string additionaInformation)
        {
            additionaInformation = Regex.Replace(additionaInformation, @"\u00A0|\u2009", " ");
            additionaInformation = additionaInformation.Replace("\n", "").Replace("\t", "").Trim();
            return additionaInformation;
        }
    }
}
