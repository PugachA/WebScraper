using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using WebScraper.Core.Extractors;
using WebScraper.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace WebScraper.Core.Factories
{
    public class PriceParserFactory
    {
        private readonly IServiceProvider servicesProvider;

        public PriceParserFactory(IServiceProvider serviceProvider)
        {
            servicesProvider = serviceProvider;
        }

        public IProductDataExtractor<T> Get<T>(Site site)
        {
            switch (site.Settings.PriceParser)
            {
                case "HtmlPriceParser":
                    return servicesProvider.GetService<HtmlExtractor>() as IProductDataExtractor<T>;
                case "MLPriceParser":
                    return servicesProvider.GetService<MLExtractor>() as IProductDataExtractor<T>;
                case "ComputerVisionParser":
                    return servicesProvider.GetService<ComputerVisionExtractor>() as IProductDataExtractor<T>;
                default:
                    throw new ArgumentException($"{site.Settings.PriceParser} тип {typeof(IProductDataExtractor<T>)} не поддерживается");
            }
        }
    }
}
