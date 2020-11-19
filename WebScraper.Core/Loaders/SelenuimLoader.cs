using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Core.Parsers;
using WebScraper.Data.Models;

namespace WebScraper.Core.Loaders
{
    public class SelenuimLoader : IDisposable, IHtmlLoader
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
                logger.LogInformation($"Секция SeleniumLoadTimoutSeconds не найдена, задаем значение по умолчанию {loadTimeoutSeconds}");
            }

            var webDriverCounts = configuration.GetSection("SeleniumWebDriverCounts").Get<int>();
            if (webDriverCounts == 0)
            {
                webDriverCounts = Environment.ProcessorCount;
                logger.LogInformation($"Секция SeleniumWebDriverCounts не найдена, задаем занчение равное количеству логических ядер={webDriverCounts}");
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

                logger.LogInformation($"Создан {nameof(ChromeDriver)}");
            }

            logger.LogInformation($"Создано {webDriverCounts} {nameof(ChromeDriver)}");
        }

        public async Task<IDocument> Load(string url, Site siteDto, CancellationToken token)
        {
            IWebDriver webDriver = null;
            try
            {
                await semaphoreSlim.WaitAsync(token);

                var parserSettings = _configuration.GetSection(siteDto.Name).Get<ParserSettings>();

                webDriver = webDriverQueue.Dequeue();
                webDriver.Url = url;

                //Чтобы дождаться прогрузки страницы
                _ = webDriver.FindElement(By.ClassName(parserSettings.Name.Trim('.')));

                logger.LogInformation($"Успешно отправлен запрос на {url}");

                var context = BrowsingContext.New(Configuration.Default);
                return await context.OpenAsync(req => req.Content(webDriver.PageSource));
            }
            finally
            {
                webDriver.Url = siteDto.BaseUrl;
                webDriverQueue.Enqueue(webDriver);
                semaphoreSlim.Release();
            }
        }

        public void Dispose()
        {
            semaphoreSlim?.Dispose();
            foreach (var webDriver in webDriverQueue)
            {
                webDriver?.Dispose();
                logger.LogInformation($"Удален {nameof(ChromeDriver)}");
            }
        }
    }
}
