﻿using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebScraper.Core.ML
{
    public class PriceData
    {
        //TODO Заменить 0 1 на true false
        [ColumnName("IsContainsPrice"), LoadColumn(0)]
        public string IsContainsPrice { get; set; }

        [ColumnName("HtmlElement"), LoadColumn(1)]
        public string HtmlElement { get; set; }
    }
}
