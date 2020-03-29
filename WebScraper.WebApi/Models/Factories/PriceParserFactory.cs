using System;

namespace WebScraper.WebApi.Models.Factories
{
    public class PriceParserFactory : IFactory<IPriceParser>
    {
        public IPriceParser Get(Site site) =>
            site.Name switch
            {
                "Beru" => new BeruPriceParser(),
                _ => throw new ArgumentException($"Не удалось создать объект {nameof(IPriceParser)} для сайта {site.Name}"),
            };
    }
}
