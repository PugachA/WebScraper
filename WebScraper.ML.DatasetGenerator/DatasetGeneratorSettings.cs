using System.Collections.Generic;
using System.Linq;
using WebScraper.Core.Parsers;

namespace WebScraper.ML.DatasetGenerator
{
    public class DataSetGeneratorSettings
    {
        public List<string> PriceTags { get; set; }
        public List<string> Regex { get; set; }
        public bool UseParserSettings { get; set; }

        public DataSetGeneratorSettings()
        {
            PriceTags = new List<string>();
            Regex = new List<string>();
            UseParserSettings = true;
        }

        public void AddParserSettings(ParserSettings parserSettings)
        {
            if (PriceTags == null)
                PriceTags = new List<string>();

            if (UseParserSettings)
            {
                foreach (var priceHtmlPath in parserSettings.PriceHtmlPath)
                    PriceTags.Add(priceHtmlPath.Split('.').Last());

                PriceTags.Add(parserSettings.DiscountHtmlPath.Split('.').Last());
            }
        }
    }
}
