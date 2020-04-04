using AngleSharp.Html.Dom;
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Models
{
    public interface IPriceParser
    {
        PriceInfo Parse(IHtmlDocument htmlDocument);
    }
}