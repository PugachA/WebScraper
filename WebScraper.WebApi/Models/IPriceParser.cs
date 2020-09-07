using AngleSharp.Html.Dom;
using WebScraper.Data.Models;

namespace WebScraper.WebApi.Models
{
    public interface IPriceParser
    {
        PriceInfo Parse(IHtmlDocument htmlDocument);
    }
}