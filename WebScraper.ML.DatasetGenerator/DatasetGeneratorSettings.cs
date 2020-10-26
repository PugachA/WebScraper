using System.Collections.Generic;
using System.Linq;
using WebScraper.Core.Parsers;

namespace WebScraper.ML.DatasetGenerator
{
    public class DataSetGeneratorSettings
    {
        public List<string> PriceTags { get; set; }
        public List<string> Regex { get; set; }

        public DataSetGeneratorSettings()
        {
            PriceTags = new List<string>();
            Regex = new List<string>();
        }

        public void AddParserSettings(ParserSettings parserSettings)
        {
            if (PriceTags == null)
                PriceTags = new List<string>();

            PriceTags.Add(parserSettings.PriceHtmlPath.Split('.').Last());
            PriceTags.Add(parserSettings.DiscountHtmlPath.Split('.').Last());
        }
    }
}
