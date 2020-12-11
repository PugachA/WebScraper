using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Core.Extensions;
using WebScraper.Core.Parsers;
using WebScraper.Data.Models;

namespace WebScraper.Core.Loaders
{
    public class PuppeteerLoader : IHtmlLoader, IScreenshotLoader, IDisposable
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
                Headless = headless,
            }).Result;

            logger.LogInformation($"Create {nameof(Browser)}");
        }

        public async Task<IDocument> LoadHtml(string requestUri, Site site, CancellationToken token)
        {
            requestUri.StringNullOrEmptyValidate(nameof(requestUri));
            site.NullValidate(nameof(site));

            var parserSettings = configuration.GetSection(site.Name).Get<ParserSettings>();

            using var page = await browser.NewPageAsync();
            await page.GoToAsync(requestUri);

            var waitingSelector = parserSettings.WaitingSelector ?? parserSettings.Name;
            await page.WaitForSelectorAsync(waitingSelector);

            logger.LogInformation($"Successfully sent request {requestUri}");

            var source = await page.GetContentAsync();

            logger.LogInformation("Successfully received page html content");

            var context = BrowsingContext.New(Configuration.Default);
            return await context.OpenAsync(req => req.Content(source));
        }

        public async Task LoadScreenshot(string outputPath, string requestUri, Site site, CancellationToken token)
        {
            outputPath.StringNullOrEmptyValidate(nameof(outputPath));
            requestUri.StringNullOrEmptyValidate(nameof(requestUri));
            site.NullValidate(nameof(site));

            using var page = await browser.NewPageAsync();
            await page.GoToAsync(requestUri);

            logger.LogInformation($"Successfully sent request {requestUri}");

            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1920,
                Height = 1080
            });

            //await page.ReloadAsync();
            await page.ScreenshotAsync(outputPath);

            logger.LogInformation($"Screenshoot successfully saved to {outputPath}");
        }

        public void Dispose()
        {
            browser?.Dispose();
            logger.LogInformation($"Delete {nameof(Browser)}");
        }
    }
}
