using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraper.Core.Extensions;
using WebScraper.Core.ML;

namespace WebScraper.Core.Parsers
{
    public class MLPriceParser : IPriceParser
    {
        private readonly PredictionEnginePool<PriceData, PricePrediction> predictionEnginePool;
        private readonly ILogger<MLPriceParser> logger;

        public MLPriceParser(PredictionEnginePool<PriceData, PricePrediction> predictionEnginePool, ILogger<MLPriceParser> logger)
        {
            this.predictionEnginePool = predictionEnginePool;
            this.logger = logger;
        }

        public async Task<PriceInfo> Parse(IDocument htmlDocument)
        {
            foreach (var priceData in GetPriceData(htmlDocument))
            {

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
                yield return new PriceData
                {
                    HtmlElement = element.OuterHtml.Replace("\n", "").Replace("\r\n", "")
                };
        }
    }
}
