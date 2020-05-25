using AngleSharp.Html.Dom;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Models
{
    public interface IHtmlLoader
    {
        Task<IHtmlDocument> Load(string requestUri, SiteDto siteDto, CancellationToken token);
    }
}