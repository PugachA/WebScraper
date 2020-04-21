using Microsoft.Extensions.Logging;
using System;
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Models.Factories
{
    public class PriceParserFactory : IFactory<IPriceParser>
    {
        private readonly ILogger _logger;

        public PriceParserFactory(ILogger logger)
        {
            _logger = logger;
        }

        public IPriceParser Get(SiteDto site) =>
            site.Name switch
            {
                "Beru" => new BeruPriceParser(_logger),
                _ => throw new ArgumentException($"Не удалось создать объект {nameof(IPriceParser)} для сайта {site.Name}"),
            };
    }
}
