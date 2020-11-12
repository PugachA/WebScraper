using Microsoft.Extensions.DependencyInjection;
using System;
using WebScraper.Core.Loaders;
using WebScraper.Data.Models;

namespace WebScraper.Core.Factories
{
    public class HtmlLoaderFactory : IFactory<IHtmlLoader>
    {
        private readonly IServiceProvider _servicesProvider;

        public HtmlLoaderFactory(IServiceProvider serviceProvider)
        {
            _servicesProvider = serviceProvider;
        }

        public IHtmlLoader Get(Site siteDto)
        {
            switch(siteDto.Settings.HtmlLoader)
            {
                case "HttpLoader":
                    return _servicesProvider.GetService<HttpLoader>();
                case "SeleniumLoader":
                    return _servicesProvider.GetService<SelenuimLoader>();
                case "PuppeteerLoader":
                    return _servicesProvider.GetService<PuppeteerLoader>();
                case "HeadlessPuppeteerLoader":
                    return _servicesProvider.GetService<HeadlessPuppeteerLoader>();
                default:
                    throw new ArgumentException($"{siteDto.Settings.HtmlLoader} тип {typeof(IHtmlLoader).Name} не поддерживается");
            }
        }
    }
}
