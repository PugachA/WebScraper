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
using System.Threading.Tasks;
using WebScraper.Core.Extensions;
using WebScraper.Core.ML;

namespace WebScraper.Core.Parsers
{
    public class MLPriceParser : PriceParser
    {
        private readonly PredictionEnginePool<PriceData, PricePrediction> predictionEnginePool;
        private readonly ILogger<MLPriceParser> logger;
        private const float PredictionLimit = 0.65F;
        private const int MaxPriceElementsInterval = 3;

        public MLPriceParser(PredictionEnginePool<PriceData, PricePrediction> predictionEnginePool, ParserSettings parserSettings, ILogger<MLPriceParser> logger) : base(parserSettings, logger)
        {
            this.predictionEnginePool = predictionEnginePool;
        }

        public override async Task<PriceInfo> Parse(IDocument htmlDocument)
        {
            string priceHtmlElement = null;
            string discountPriceHtmlElement = null;
            int count = 0;

            foreach (var priceData in GetPriceData(htmlDocument))
            {
                var pricePrediction = predictionEnginePool.Predict(modelName: "PriceDetectionModel", example: priceData);

                if (!bool.TryParse(pricePrediction.Prediction, out bool isPrice))
                    throw new InvalidCastException($"Can not convert {nameof(pricePrediction.Prediction)}={pricePrediction.Prediction} to {typeof(bool)}");

                if (isPrice && pricePrediction.Score[0] >= PredictionLimit)
                {
                    if (priceHtmlElement != null && count < MaxPriceElementsInterval)
                    {
                        discountPriceHtmlElement = priceData.HtmlElement;
                        logger.LogInformation($"Detect {nameof(discountPriceHtmlElement)}={discountPriceHtmlElement} with {pricePrediction.Score}=[{string.Join(';', pricePrediction.Score)}]");
                    }

                    if (priceHtmlElement != null)
                    {
                        priceHtmlElement = priceData.HtmlElement;
                        logger.LogInformation($"Detect {nameof(priceHtmlElement)}={priceHtmlElement} with {pricePrediction.Score}=[{string.Join(';', pricePrediction.Score)}]");
                    }

                    count = 0;
                }

                count++;
            }

            var context = BrowsingContext.New(Configuration.Default);
            var priceDocument = await context.OpenAsync(req => req.Content(priceHtmlElement));
            string price = ExtractPrice(priceDocument.TextContent);

            var discountPriceDocument = await context.OpenAsync(req => req.Content(discountPriceHtmlElement));
            string discountPrice = ExtractPrice(discountPriceDocument.TextContent);

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
            string price = null;

            if (!string.IsNullOrEmpty(priceString))
                price = TransformPrice(priceString);

            price ??= base.ExtractPrice(price);

            return price;
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
                yield return new PriceData
                {
                    HtmlElement = element.OuterHtml.Replace("\n", "").Replace("\r\n", "")
                };
        }
    }
}
