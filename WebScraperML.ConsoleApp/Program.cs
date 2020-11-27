// This file was auto-generated by ML.NET Model Builder. 

using System;
using WebScraperML.Model;

namespace WebScraperML.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create single instance of sample data from first line of dataset for model input
            ModelInput sampleData = new ModelInput()
            {
                HtmlElement = @"<meta charset=\"utf - 8\">",
            };

            // Make a single prediction on the sample data and print results
            var predictionResult = ConsumeModel.Predict(sampleData);

            Console.WriteLine("Using model to make single prediction -- Comparing actual IsContainsPrice with predicted IsContainsPrice from sample data...\n\n");
            Console.WriteLine($"HtmlElement: {sampleData.HtmlElement}");
            Console.WriteLine($"\n\nPredicted IsContainsPrice value {predictionResult.Prediction} \nPredicted IsContainsPrice scores: [{String.Join(",", predictionResult.Score)}]\n\n");
            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }
    }
}
