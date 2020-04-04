using System;
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Models.Factories
{
    public class PriceParserFactory : IFactory<IPriceParser>
    {
        public IPriceParser Get(SiteDto site) =>
            site.Name switch
            {
                "Beru" => new BeruPriceParser(),
                _ => throw new ArgumentException($"Не удалось создать объект {nameof(IPriceParser)} для сайта {site.Name}"),
            };
    }
}
