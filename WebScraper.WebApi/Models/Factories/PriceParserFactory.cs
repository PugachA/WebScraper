using Microsoft.Extensions.Logging;
using System;
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Models.Factories
{
    public class PriceParserFactory : IFactory<IPriceParser>
    {
        private readonly ILogger<PriceParserFactory> _logger;

        public PriceParserFactory(ILogger<PriceParserFactory> logger)
        {
            _logger = logger;
        }

        public IPriceParser Get(SiteDto site) =>
            site.Name switch
            {
                "Beru" => new BeruPriceParser(_logger),
                //"YandexMarket" => new YandexMarketParser(_logger),
                _ => throw new ArgumentException($"Не удалось создать объект {nameof(IPriceParser)} для сайта {site.Name}"),
            };
    }
}
