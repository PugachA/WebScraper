using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebScraper.Data.Models;

namespace WebScraper.Core.Extractors
{
    public abstract class ProductDataExtractor<T> : IProductDataExtractor<T>
    {
        protected readonly ILogger<ProductDataExtractor<T>> logger;

        protected ProductDataExtractor(ILogger<ProductDataExtractor<T>> logger)
        {
            this.logger = logger;
        }

        public async Task<ProductData> Extract(T inputData, ExtractorSettings parserSettings)
        {
            if (IsCaughtByCaptcha(inputData, parserSettings))
                throw new AggregateException($"Caught by CAPTCHA");

            var name = await ExtractName(inputData, parserSettings);
            var (price, discountPrice) = await ExtractPrice(inputData, parserSettings);
            var additionalInformation = await ExtractAdditionalInformation(inputData, parserSettings);

            if (discountPrice == null && price == null)
            {
                additionalInformation = await ExtractOutofstockInformation(inputData, parserSettings);

                if (additionalInformation is null)
                    throw new FormatException("Unknown error while extract product data");
                else
                    logger.LogInformation($"The item may be out of stock or has been removed. Info: {additionalInformation}");
            }

            return new ProductData
            {
                Name = name,
                Price = price,
                DiscountPrice = discountPrice,
                AdditionalInformation = additionalInformation,
                Date = DateTime.Now
            };
        }

        protected abstract Task<string> ExtractName(T inputData, ExtractorSettings parserSettings);
        protected abstract Task<(decimal? price, decimal? discountPrice)> ExtractPrice(T inputData, ExtractorSettings parserSettings);
        protected abstract Task<string> ExtractAdditionalInformation(T inputData, ExtractorSettings parserSettings);
        protected abstract Task<string> ExtractOutofstockInformation(T inputData, ExtractorSettings parserSettings);

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

        protected virtual bool IsCaughtByCaptcha(T inputData, ExtractorSettings parserSettings) => false;
    }
}
