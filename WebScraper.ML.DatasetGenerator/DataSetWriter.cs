using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace WebScraper.ML.DatasetGenerator
{
    public class DataSetWriter
    {
        private readonly string _dataSetPath;
        private readonly CsvConfiguration _csvConfiguration;

        public DataSetWriter() : this($"DataSets/{DateTime.Now:yyyy-MM-ddTHH-mm-ss}_dataset.csv")
        { }

        public DataSetWriter(string dataSetPath)
        {
            this._dataSetPath = dataSetPath;

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
            csvConfiguration.Delimiter = ",";
            csvConfiguration.ShouldQuote = (a, b) => true;
            csvConfiguration.TypeConverterCache.AddConverter<bool>(new BinaryBooleanConverter());

            FileInfo fileInfo = new FileInfo(_dataSetPath);
            if (!fileInfo.Directory.Exists)
                Directory.CreateDirectory(fileInfo.DirectoryName);

            using StreamWriter streamWriter = new StreamWriter(_dataSetPath);
            using CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration);
            csvWriter.WriteHeader<HtmlDataSet>();
            csvWriter.NextRecord();

            csvConfiguration.HasHeaderRecord = false;
            this._csvConfiguration = csvConfiguration;
        }

        public void AppendRecords(IEnumerable<HtmlDataSet> htmlDataSets)
        {
            using StreamWriter streamWriter = new StreamWriter(_dataSetPath, append: true);
            using CsvWriter csvWriter = new CsvWriter(streamWriter, _csvConfiguration);
            csvWriter.WriteRecords(htmlDataSets);
        }
    }
}
