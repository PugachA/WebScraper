using AngleSharp.Dom;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Data.Models;

namespace WebScraper.Core.Loaders
{
    public interface IHtmlLoader
    {
        Task<IDocument> LoadHtml(string requestUri, Site siteDto, CancellationToken token);
    }
}