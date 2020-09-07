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
            if (siteDto.Settings.UseSeleniumService)
                return _servicesProvider.GetService<SelenuimService>();
            else
                return _servicesProvider.GetService<HtmlLoader>();
        }
    }
}
