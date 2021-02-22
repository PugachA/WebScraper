using System.Collections.Generic;

namespace WebScraper.Core.Extractors
{
    public class ExtractorSettings
    {
        public string Name { get; set; }
        public string WaitingSelector { get; set; }
        public string[] PriceHtmlPath { get; set; }
        public string DiscountHtmlPath { get; set; }
        public string OutOfStockHtmlPath { get; set; }
        public Dictionary<string, string> AdditionalInformation { get; set; }
    }
}
