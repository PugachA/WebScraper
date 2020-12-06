using AngleSharp.Dom;
using System.Threading.Tasks;

namespace WebScraper.Core.Parsers
{
    public interface IPriceParser<T>
    {
        Task<PriceInfo> Parse(T inputData, ParserSettings parserSettings);
    }
}