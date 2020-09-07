using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebScraper.ML.DatasetGenerator
{
    class BinaryBooleanConverter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return Convert.ToBoolean(value) ? "1" : "0";
        }
    }
}
