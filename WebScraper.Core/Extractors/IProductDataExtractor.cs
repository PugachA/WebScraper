using AngleSharp.Dom;
using System.Threading.Tasks;
using WebScraper.Data.Models;

namespace WebScraper.Core.Extractors
{
    public interface IProductDataExtractor<T>
    {
        Task<ProductData> Extract(T inputData, ExtractorSettings parserSettings);
    }
}