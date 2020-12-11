using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using WebScraper.Core.Parsers;
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

        public IPriceParser<T> Get<T>(Site site)
        {
            switch (site.Settings.PriceParser)
            {
                case "HtmlPriceParser":
                    return servicesProvider.GetService<HtmlPriceParser>() as IPriceParser<T>;
                case "MLPriceParser":
                    return servicesProvider.GetService<MLPriceParser>() as IPriceParser<T>;
                case "ComputerVisionParser":
                    return servicesProvider.GetService<ComputerVisionParser>() as IPriceParser<T>;
                default:
                    throw new ArgumentException($"{site.Settings.PriceParser} тип {typeof(IPriceParser<T>)} не поддерживается");
            }
        }
    }
}
