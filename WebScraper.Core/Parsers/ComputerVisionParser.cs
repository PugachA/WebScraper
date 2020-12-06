using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebScraper.Core.Parsers
{
    public class ComputerVisionParser : PriceParser<string>
    {
        public ComputerVisionParser(ILogger<PriceParser<string>> logger) : base(logger)
        {
        }

        public override Task<PriceInfo> Parse(string inputData, ParserSettings parserSettings)
        {
            throw new NotImplementedException();
        }
    }
}
