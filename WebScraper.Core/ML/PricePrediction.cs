using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebScraper.Core.ML
{
    public class PricePrediction
    {
        [ColumnName("PredictedLabel")]
        public string Prediction { get; set; }
        public float[] Score { get; set; }
    }
}
