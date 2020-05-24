using AngleSharp;
using AngleSharp.Common;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebScraper.WebApi.Models
{
    public class SelenuimService : IDisposable, IHtmlLoader
    {
        private readonly Queue<IWebDriver> webDriverQueue;
        private readonly SemaphoreSlim semaphoreSlim;
        private readonly ILogger<SelenuimService> logger;

        public SelenuimService(Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<SelenuimService> logger)
        {
            this.logger = logger;

            var webDriverCounts = configuration.GetSection("SeleniumWebDriverCounts").Get<int>();
            if (webDriverCounts == 0)
                webDriverCounts = Environment.ProcessorCount;

            semaphoreSlim = new SemaphoreSlim(webDriverCounts, webDriverCounts);

            webDriverQueue = new Queue<IWebDriver>();
            for (int i = 0; i < webDriverCounts; i++)
            {
                var chromeDriver = new ChromeDriver();
                webDriverQueue.Enqueue(chromeDriver);

                logger.LogInformation($"Создан {nameof(ChromeDriver)}");
            }

            logger.LogInformation($"Создано {webDriverCounts} {nameof(ChromeDriver)}");
        }

        public async Task<IHtmlDocument> Load(string url, CancellationToken token)
        {
            await semaphoreSlim.WaitAsync(token);

            IWebDriver webDriver = null;
            try
            {
                webDriver = webDriverQueue.Dequeue();
                webDriver.Url = url;

                var config = Configuration.Default;
                var context = BrowsingContext.New(config);
                var parser = context.GetService<IHtmlParser>();
                var document = parser.ParseDocument(webDriver.PageSource);

                logger.LogInformation($"Успешно отправлен запрос на {url}");

                return document;
            }
            finally
            {
                semaphoreSlim.Release();
                webDriverQueue.Enqueue(webDriver);
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
