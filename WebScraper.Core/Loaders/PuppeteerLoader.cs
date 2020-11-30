using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Core.Parsers;
using WebScraper.Data.Models;

namespace WebScraper.Core.Loaders
{
    public class PuppeteerLoader : IHtmlLoader, IDisposable
    {
        private readonly ILogger<PuppeteerLoader> logger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration configuration;
        private readonly Browser browser;

        public PuppeteerLoader(Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<PuppeteerLoader> logger) : this(configuration, logger, false)
        { }

        protected PuppeteerLoader(Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<PuppeteerLoader> logger, bool headless)
        {
            this.logger = logger;
            this.configuration = configuration;

            _ = new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision).Result;

            browser = Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = headless
            }).Result;

            logger.LogInformation($"Создан {nameof(Browser)}");
        }

        public async Task<IDocument> Load(string requestUri, Site siteDto, CancellationToken token)
        {
            var parserSettings = configuration.GetSection(siteDto.Name).Get<ParserSettings>();

            using var page = await browser.NewPageAsync();
            await page.GoToAsync(requestUri);

            var waitingSelector = parserSettings.WaitingSelector ?? parserSettings.Name;
            await page.WaitForSelectorAsync(waitingSelector);

            var source = await page.GetContentAsync();
            logger.LogInformation($"Успешно отправлен запрос на {requestUri}");

            var context = BrowsingContext.New(Configuration.Default);
            return await context.OpenAsync(req => req.Content(source));
        }

        public void Dispose()
        {
            browser?.Dispose();
            logger.LogInformation($"Удален {nameof(Browser)}");
        }
    }
}
