using AngleSharp.Html.Dom;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Data.Models;

namespace WebScraper.WebApi.Models
{
    public interface IHtmlLoader
    {
        Task<IHtmlDocument> Load(string requestUri, Site siteDto, CancellationToken token);
    }
}