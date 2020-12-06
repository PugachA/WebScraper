using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebScraper.Core.Parsers
{
    public abstract class PriceParser<T> : IPriceParser<T>
    {
        protected readonly ILogger<PriceParser<T>> logger;

        protected PriceParser(ILogger<PriceParser<T>> logger)
        {
            this.logger = logger;
        }

        public abstract Task<PriceInfo> Parse(T inputData, ParserSettings parserSettings);

        protected virtual string TransformPrice(string priceString)
        {
            var price = Regex.Replace(priceString, @"\s|\u00A0|\u2009", String.Empty);
            price = price.Replace(",", ".").Trim();
            return price;
        }

        protected virtual string ExtractPrice(string priceString)
        {
            Regex regex = new Regex(@"\d*\.?\d{1,2}");
            return regex.Match(priceString).Value;
        }
    }
}
