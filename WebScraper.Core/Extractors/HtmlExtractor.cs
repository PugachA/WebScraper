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
using WebScraper.Data.Models;

namespace WebScraper.Core.Extractors
{
    public class HtmlExtractor : ProductDataExtractor<IDocument>
    {
        public HtmlExtractor(ILogger<HtmlExtractor> logger) : base(logger)
        { }

        protected override Task<string> ExtractName(IDocument inputData, ExtractorSettings parserSettings)
        {
            var nameElement = inputData.QuerySelectorAll(parserSettings.Name).FirstOrDefault();
            logger.LogInformation($"The processed part of the document by product name: {nameElement?.OuterHtml}");

            return Task.FromResult(nameElement?.TextContent.Trim());
        }

        protected override Task<(decimal? price, decimal? discountPrice)> ExtractPrice(IDocument inputData, ExtractorSettings parserSettings)
        {
            var discountPriceElement = inputData.QuerySelectorAll(parserSettings.DiscountHtmlPath).FirstOrDefault();
            logger.LogInformation($"The processed part of the document by discount: {discountPriceElement?.OuterHtml}");

            var discountPrice = discountPriceElement?.TextContent;

            string price = default;
            foreach (var priceHtmlPath in parserSettings.PriceHtmlPath)
            {
                var priceElement = inputData.QuerySelectorAll(priceHtmlPath).FirstOrDefault();
                logger.LogInformation($"The processed part of the document by price {priceElement?.OuterHtml}");

                price = priceElement?.TextContent;

                if (!string.IsNullOrEmpty(price))
                    break;
            }

            if (discountPrice == null && price == null)
                return Task.FromResult<(decimal? price, decimal? discountPrice)>((null, null));

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

            return Task.FromResult(((decimal?)priceValue, discountPriceValue));
        }

        protected override Task<string> ExtractAdditionalInformation(IDocument htmlDocument, ExtractorSettings parserSettings)
        {
            if (parserSettings.AdditionalInformation == null)
                return Task.FromResult<string>(null);

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

            return Task.FromResult(additionalInformationString);
        }

        protected override Task<string> ExtractOutofstockInformation(IDocument inputData, ExtractorSettings parserSettings) 
            => Task.FromResult(inputData.QuerySelectorAll(parserSettings.OutOfStockHtmlPath).FirstOrDefault()?.TextContent);

        protected override bool IsCaughtByCaptcha(IDocument inputData, ExtractorSettings parserSettings)
        {
            if(inputData.Title == "Ой!")
            {
                logger.LogError($"Попали на капчу {inputData.Source.Text}");
                return true;
            }

            return false;
        }

        private string TransformAdditionalInformation(string additionaInformation)
        {
            additionaInformation = Regex.Replace(additionaInformation, @"\u00A0|\u2009", " ");
            additionaInformation = additionaInformation.Replace("\n", "").Replace("\t", "").Trim();
            return additionaInformation;
        }
    }
}
