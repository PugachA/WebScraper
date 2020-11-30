using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebScraper.Core.Extensions;
using WebScraper.Core.ML;

namespace WebScraper.Core.Parsers
{
    public class MLPriceParser : PriceParser
    {
        private readonly PredictionEnginePool<PriceData, PricePrediction> predictionEnginePool;
        private const float PredictionLimit = 0.65F;
        private const int MaxPriceElementsInterval = 3;

        public MLPriceParser(PredictionEnginePool<PriceData, PricePrediction> predictionEnginePool, ILogger<MLPriceParser> logger) : base(logger)
        {
            this.predictionEnginePool = predictionEnginePool;
        }

        public override async Task<PriceInfo> Parse(IDocument htmlDocument, ParserSettings parserSettings)
        {
            string priceHtmlElement = null;
            string discountPriceHtmlElement = null;
            int count = 0;

            foreach (var priceData in GetPriceData(htmlDocument))
            {
                var pricePrediction = predictionEnginePool.Predict(modelName: "PriceDetectionModel", example: priceData);

                if (!bool.TryParse(pricePrediction.Prediction, out bool isPrice))
                    throw new InvalidCastException($"Can not convert {pricePrediction.Prediction} to {typeof(bool)}");

                if (isPrice)
                {
                    if (priceHtmlElement != null && count < MaxPriceElementsInterval)
                    {
                        discountPriceHtmlElement = priceData.HtmlElement;
                        logger.LogInformation($"Detect {nameof(discountPriceHtmlElement)}={discountPriceHtmlElement} with {pricePrediction.Score}=[{string.Join(';', pricePrediction.Score)}]");
                    }

                    if (priceHtmlElement == null)
                    {
                        priceHtmlElement = priceData.HtmlElement;
                        logger.LogInformation($"Detect {nameof(priceHtmlElement)}={priceHtmlElement} with {pricePrediction.Score}=[{string.Join(';', pricePrediction.Score)}]");
                    }

                    count = 0;
                }

                count++;
            }

            if (priceHtmlElement is null && discountPriceHtmlElement is null)
                return new PriceInfo(null, null, null, null);

            string price = ExtractPrice(priceHtmlElement);
            string discountPrice = ExtractPrice(discountPriceHtmlElement);

            if (!decimal.TryParse(price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal priceValue))
                throw new InvalidCastException($"Can not convert {nameof(price)}={price} to {typeof(decimal)}");

            if (!decimal.TryParse(discountPrice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal discountPriceTemp) && discountPrice != null)
                throw new InvalidCastException($"Can not convert {nameof(discountPrice)}={discountPrice} to {typeof(decimal)}");

            decimal? discountPriceValue = null;
            if (discountPrice != null && discountPriceTemp < priceValue)
            {
                decimal temp = priceValue;
                priceValue = (decimal)discountPriceValue;
                discountPriceValue = temp;
            }

            //TODO Сделать отдельные методы extract для каждого поля с возможностью переопределения
            return new PriceInfo(priceValue, discountPriceValue, null, null);
        }

        protected override string ExtractPrice(string priceString)
        {
            if (!string.IsNullOrEmpty(priceString))
            {
                priceString = TransformPrice(priceString);
                return base.ExtractPrice(priceString);
            }

            return null;
        }

        private IEnumerable<PriceData> GetPriceData(IDocument document)
        {
            var htmlElements = document.QuerySelectorAll("*").Where(el => el.ChildElementCount == 0 && !String.IsNullOrEmpty(el.OuterHtml));
            htmlElements = htmlElements.OfTypes(new Type[]
            {
                typeof(IHtmlSpanElement),
                typeof(IHtmlDivElement),
                typeof(IHtmlMetaElement),
                typeof(IHtmlListItemElement)
            });

            foreach (var element in htmlElements)
                if (!string.IsNullOrEmpty(element.TextContent))
                    yield return new PriceData
                    {
                        HtmlElement = HtmlElementTransform(element.OuterHtml),
                        ClassName = element.ClassName,
                        HtmlElementName = element.LocalName
                    };
        }

        private string HtmlElementTransform(string textContent)
        {
            return textContent.Replace("\n", "").Replace("\r\n", "").Replace("&nbsp;", " ").Replace("<!-- -->", "");
        }
    }
}
