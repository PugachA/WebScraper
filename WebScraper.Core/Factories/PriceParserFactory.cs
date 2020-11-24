using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using WebScraper.Core.Parsers;
using WebScraper.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace WebScraper.Core.Factories
{
    public class PriceParserFactory : IFactory<IPriceParser>
    {
        private readonly IServiceProvider servicesProvider;

        public PriceParserFactory(IServiceProvider serviceProvider)
        {
            servicesProvider = serviceProvider;
        }

        public IPriceParser Get(Site site)
        {
            switch (site.Settings.PriceParser)
            {
                case "PriceParser":
                    return servicesProvider.GetService<PriceParser>();
                case "MLPriceParser":
                    return servicesProvider.GetService<MLPriceParser>();
                default:
                    throw new ArgumentException($"{site.Settings.PriceParser} тип {typeof(IPriceParser)} не поддерживается");
            }
        }
    }
}
