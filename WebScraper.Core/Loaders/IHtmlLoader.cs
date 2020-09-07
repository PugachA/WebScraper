using AngleSharp.Html.Dom;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Data.Models;

namespace WebScraper.Core.Loaders
{
    public interface IHtmlLoader
    {
        Task<IHtmlDocument> Load(string requestUri, Site siteDto, CancellationToken token);
    }
}