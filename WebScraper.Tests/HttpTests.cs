using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebScraper.Tests
{
    public class HttpTests
    {
        private Mock<ILogger> mockLogger;

        [SetUp]
        public void Setup()
        {
            mockLogger = new Mock<ILogger>();
        }

        [Test]
        public async Task PuppeteerTest()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false
            });
            var page = await browser.NewPageAsync();
            await page.GoToAsync("https://beru.ru/product/pla-prutok-bestfilament-1-75-mm-belyi-0-5-kg/100346967800?show-uid=16005480854223810791106020&offerid=zBh1lM8pPGit3h1MVHmOkQ");
            var str = await page.GetContentAsync();

            await browser.CloseAsync();
        }

        [Test]
        public void SeleniumTest()
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            //chromeOptions.AddArguments(new List<string>() {
            //    //    "--silent-launch",
            //    //    "--no-startup-window",
            //    //    "no-sandbox",
            //       "headless"
            //    //    "disable-gpu"
            //    });
            //chromeOptions.AddArgument(@"user-data-dir=C:\Users\foton\AppData\Local\Temp\scoped_dir22284_1666492972");

            //var chromeDriverService = ChromeDriverService.CreateDefaultService();
            //chromeDriverService.HideCommandPromptWindow = true;
            var chromeDriver = new ChromeDriver(chromeOptions);
            chromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            chromeDriver.Url = "https://beru.ru/product/pla-prutok-bestfilament-1-75-mm-belyi-0-5-kg/100346967800?show-uid=16005480854223810791106020&offerid=zBh1lM8pPGit3h1MVHmOkQ";
            var str = chromeDriver.PageSource;
            chromeDriver.Close();
        }

    }
}
