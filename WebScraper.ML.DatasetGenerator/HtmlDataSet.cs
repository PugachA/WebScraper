using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebScraper.ML.DatasetGenerator
{
    public class HtmlDataSet
    {
        public bool IsContainsPrice { get; set; }
        public string HtmlElement { get; set; }
    }
}
