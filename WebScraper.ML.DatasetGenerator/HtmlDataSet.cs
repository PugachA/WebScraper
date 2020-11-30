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
        public string ClassName { get; set; }
        public string HtmlElementName { get; set; }
        //public int NestingDepth { get; set; }
    }
}
