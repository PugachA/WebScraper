using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebScraper.WebApi.Models
{
    public class ParserSettings
    {
        public string Name { get; set; }
        public string PriceHtmlPath { get; set; }
        public string DiscountHtmlPath { get; set; }
        public string OutOfStockHtmlPath { get; set; }
        public Dictionary<string, string> AdditionalInformation { get; set; }
    }
}
