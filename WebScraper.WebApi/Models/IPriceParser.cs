using AngleSharp.Html.Dom;

namespace WebScraper.WebApi.Models
{
    public interface IPriceParser
    {
        PriceInfo Parse(IHtmlDocument htmlDocument);
    }
}