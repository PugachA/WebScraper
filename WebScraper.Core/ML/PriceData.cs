using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebScraper.Core.ML
{
    public class PriceData
    {
        [ColumnName("IsContainsPrice"), LoadColumn(0)]
        public string IsContainsPrice { get; set; }

        [ColumnName("HtmlElement"), LoadColumn(1)]
        public string HtmlElement { get; set; }

        [ColumnName("ClassName"), LoadColumn(2)]
        public string ClassName { get; set; }

        [ColumnName("HtmlElementName"), LoadColumn(3)]
        public string HtmlElementName { get; set; }
    }
}
