using AngleSharp.Dom;
using System.Threading.Tasks;

namespace WebScraper.Core.Parsers
{
    public interface IPriceParser
    {
        Task<PriceInfo> Parse(IDocument htmlDocument, ParserSettings parserSettings);
    }
}