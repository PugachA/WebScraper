using AngleSharp.Html.Dom;
using WebScraper.Data.Models;

namespace WebScraper.Core.Parsers
{
    public interface IPriceParser
    {
        PriceInfo Parse(IHtmlDocument htmlDocument);
    }
}