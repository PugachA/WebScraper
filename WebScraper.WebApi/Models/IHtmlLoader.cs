using AngleSharp.Html.Dom;
using System.Threading;
using System.Threading.Tasks;

namespace WebScraper.WebApi.Models
{
    public interface IHtmlLoader
    {
        Task<IHtmlDocument> Load(string requestUri, CancellationToken token);
    }
}