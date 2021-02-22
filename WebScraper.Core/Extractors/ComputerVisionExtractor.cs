using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using WebScraper.Core.CV;

namespace WebScraper.Core.Extractors
{
    public class ComputerVisionExtractor : ProductDataExtractor<string>
    {
        private readonly PredictionEnginePool<ModelInput, ModelOutput> predictionEnginePool;
        private readonly IConfiguration configuration;

        public ComputerVisionExtractor(PredictionEnginePool<ModelInput, ModelOutput> predictionEnginePool, ILogger<ProductDataExtractor<string>> logger, IConfiguration configuration) : base(logger)
        {
            this.predictionEnginePool = predictionEnginePool;
            this.configuration = configuration;
        }

        protected override Task<string> ExtractAdditionalInformation(string inputData, ExtractorSettings parserSettings) => Task.FromResult<string>(null);

        protected override Task<string> ExtractName(string inputData, ExtractorSettings parserSettings) => Task.FromResult<string>(null);

        protected override Task<string> ExtractOutofstockInformation(string inputData, ExtractorSettings parserSettings) => Task.FromResult<string>(null);

        protected override Task<(decimal? price, decimal? discountPrice)> ExtractPrice(string inputData, ExtractorSettings parserSettings)
        {
            //TODO Сделать вырез цены из скрипшота
            ModelInput sampleData = new ModelInput { ImageSource = inputData };

            // Make a single prediction on the sample data and print results
            var prediction = predictionEnginePool.Predict(modelName: "CVPriceDetectionModel", example: sampleData);

            var priceBox = prediction.BoundingBoxes?.Where(p => p.Label == "price").OrderByDescending(p => p.Score).First();

            using Bitmap source = new Bitmap(sampleData.ImageSource);
            using Bitmap resizeImage = ResizeBitmap(source, 800, 600);
            Rectangle section = new Rectangle(new Point((int)priceBox.Left, (int)priceBox.Top), new Size((int)priceBox.Right - (int)priceBox.Left, (int)priceBox.Bottom - (int)priceBox.Top));

            using Bitmap priceImage = CropImage(resizeImage, section);
            var priceImagePath = Path.Combine(configuration.GetValue<string>("ImagesFolder"), $"{Path.GetFileNameWithoutExtension(inputData)}-price.png");
            priceImage.Save(priceImagePath, System.Drawing.Imaging.ImageFormat.Png);

            var root = configuration.GetValue<string>(WebHostDefaults.ContentRootKey);
            using var tesseractEngine = new TesseractEngine($"{root}/CV/Tesseract", "eng+rus", EngineMode.Default);
            using var image = Pix.LoadFromFile(priceImagePath);
            using var page = tesseractEngine.Process(image);
            var price = page.GetText();

            File.Delete(priceImagePath);

            if (price is null)
                throw new AggregateException($"Can not find content from image {inputData}");

            price = TransformPrice(price);
            price = ExtractPrice(price);

            if (!decimal.TryParse(price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal priceValue))
                throw new InvalidCastException($"Can not convert {nameof(price)}={price} to {typeof(decimal)}");

            return Task.FromResult(((decimal?)priceValue, (decimal?)null));
        }

        private Bitmap CropImage(Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
        }

        private Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }

            return result;
        }
    }
}
