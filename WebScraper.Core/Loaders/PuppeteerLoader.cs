using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<IHtmlDocument> Load(string requestUri, Site siteDto, CancellationToken token)
        {
            var parserSettings = configuration.GetSection(siteDto.Name).Get<ParserSettings>();

            using var page = await browser.NewPageAsync();
            await page.GoToAsync(requestUri);

            var waitingSelector = parserSettings.WaitingSelector ?? parserSettings.Name;
            await page.WaitForSelectorAsync(waitingSelector);

            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(await page.GetContentAsync());

            logger.LogInformation($"Успешно отправлен запрос на {requestUri}");

            return document;
        }

        public void Dispose()
        {
            browser?.Dispose();
            logger.LogInformation($"Удален {nameof(Browser)}");
        }
    }
}
