using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebScraper.Core.Parsers;

namespace WebScraper.ML.DatasetGenerator
{
    public class DataSetGeneratorSettings
    {
        public List<string> PriceTags { get; set; }

        public void AddParserSettings(ParserSettings parserSettings)
        {
            PriceTags.Add(parserSettings.PriceHtmlPath.Split('.').Last());
            PriceTags.Add(parserSettings.DiscountHtmlPath.Split('.').Last());
        }
    }
}
