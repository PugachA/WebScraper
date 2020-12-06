using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Core.Extensions;
using WebScraper.Core.Parsers;
using WebScraper.Data.Models;

namespace WebScraper.Core.Loaders
{
    public class SelenuimLoader : IHtmlLoader, IScreenshotLoader, IDisposable
    {
        private readonly Queue<IWebDriver> webDriverQueue;
        private readonly SemaphoreSlim semaphoreSlim;
        private readonly ILogger<SelenuimLoader> logger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public SelenuimLoader(Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<SelenuimLoader> logger)
        {
            this.logger = logger;
            this._configuration = configuration;

            var loadTimeoutSeconds = configuration.GetSection("SeleniumLoadTimoutSeconds").Get<int>();
            if (loadTimeoutSeconds == 0)
            {
                loadTimeoutSeconds = 30;
                logger.LogInformation($"Section SeleniumLoadTimoutSeconds not found, set default value {loadTimeoutSeconds}");
            }

            var webDriverCounts = configuration.GetSection("SeleniumWebDriverCounts").Get<int>();
            if (webDriverCounts == 0)
            {
                webDriverCounts = Environment.ProcessorCount;
                logger.LogInformation($"Section SeleniumWebDriverCounts not foundа, set value equal to the counts of logical cores={webDriverCounts}");
            }

            semaphoreSlim = new SemaphoreSlim(webDriverCounts, webDriverCounts);

            webDriverQueue = new Queue<IWebDriver>();
            for (int i = 0; i < webDriverCounts; i++)
            {
                ChromeOptions chromeOptions = new ChromeOptions();
                //chromeOptions.AddArguments(new List<string>() {
                //    "--silent-launch",
                //    "--no-startup-window",
                //    "no-sandbox",
                //    "headless"
                //    "disable-gpu"
                //});
                //chromeOptions.AddArgument(@"user-data-dir=C:\Users\foton\AppData\Local\Temp\scoped_dir22284_1666492972");

                //var chromeDriverService = ChromeDriverService.CreateDefaultService();
                //chromeDriverService.HideCommandPromptWindow = true;
                var chromeDriver = new ChromeDriver(chromeOptions);
                chromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(loadTimeoutSeconds);
                webDriverQueue.Enqueue(chromeDriver);

                logger.LogInformation($"Create {nameof(ChromeDriver)}");
            }

            logger.LogInformation($"Created {webDriverCounts} instanse of {nameof(ChromeDriver)}");
        }

        public async Task<IDocument> LoadHtml(string requestUri, Site site, CancellationToken token)
        {
            requestUri.StringNullOrEmptyValidate(nameof(requestUri));
            site.NullValidate(nameof(site));

            IWebDriver webDriver = null;
            try
            {
                await semaphoreSlim.WaitAsync(token);

                var parserSettings = _configuration.GetSection(site.Name).Get<ParserSettings>();

                webDriver = webDriverQueue.Dequeue();
                webDriver.Url = requestUri;

                //Чтобы дождаться прогрузки страницы
                _ = webDriver.FindElement(By.ClassName(parserSettings.Name.Trim('.')));

                logger.LogInformation($"Successfully sent request {requestUri}");

                var context = BrowsingContext.New(Configuration.Default);
                return await context.OpenAsync(req => req.Content(webDriver.PageSource));
            }
            finally
            {
                webDriver.Url = site.BaseUrl;
                webDriverQueue.Enqueue(webDriver);
                semaphoreSlim.Release();
            }
        }

        public async Task LoadScreenshot(string outputPath, string requestUri, Site site, CancellationToken token)
        {
            outputPath.StringNullOrEmptyValidate(nameof(outputPath));
            requestUri.StringNullOrEmptyValidate(nameof(requestUri));
            site.NullValidate(nameof(site));

            IWebDriver webDriver = null;
            try
            {
                await semaphoreSlim.WaitAsync(token);

                var parserSettings = _configuration.GetSection(site.Name).Get<ParserSettings>();

                webDriver = webDriverQueue.Dequeue();
                webDriver.Url = requestUri;

                //Чтобы дождаться прогрузки страницы
                _ = webDriver.FindElement(By.ClassName(parserSettings.Name.Trim('.')));

                logger.LogInformation($"Successfully sent request {requestUri}");

                Screenshot ss = ((ITakesScreenshot)webDriver).GetScreenshot();
                ss.SaveAsFile(outputPath, ExtractScreenshotFormat(outputPath));

                logger.LogInformation($"Screenshoot successfully saved to {outputPath}");
            }
            finally
            {
                webDriver.Url = site.BaseUrl;
                webDriverQueue.Enqueue(webDriver);
                semaphoreSlim.Release();
            }
        }

        private ScreenshotImageFormat ExtractScreenshotFormat(string outputPath)
        {
            var extension = Path.GetExtension(outputPath);
            switch (extension)
            {
                case ".png":
                    return ScreenshotImageFormat.Png;
                case ".jpeg":
                    return ScreenshotImageFormat.Jpeg;
                default:
                    throw new ArgumentException($"Extension {extension} not supported");
            }
        }

        public void Dispose()
        {
            semaphoreSlim?.Dispose();
            foreach (var webDriver in webDriverQueue)
            {
                webDriver?.Dispose();
                logger.LogInformation($"Delete {nameof(ChromeDriver)}");
            }
        }
    }
}
