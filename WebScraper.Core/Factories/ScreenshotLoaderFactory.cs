using System;
using WebScraper.Core.Loaders;
using WebScraper.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace WebScraper.Core.Factories
{
    public class ScreenshotLoaderFactory : IFactory<IScreenshotLoader>
    {
        private readonly IServiceProvider _servicesProvider;

        public ScreenshotLoaderFactory(IServiceProvider serviceProvider)
        {
            _servicesProvider = serviceProvider;
        }

        public IScreenshotLoader Get(Site site)
        {
            switch (site.Settings.HtmlLoader)
            {
                case "SeleniumLoader":
                    return _servicesProvider.GetService<SelenuimLoader>();
                case "PuppeteerLoader":
                    return _servicesProvider.GetService<PuppeteerLoader>();
                case "HeadlessPuppeteerLoader":
                    return _servicesProvider.GetService<HeadlessPuppeteerLoader>();
                default:
                    throw new ArgumentException($"{site.Settings.HtmlLoader} тип {typeof(IScreenshotLoader).Name} не поддерживается");
            }
        }
    }
}
