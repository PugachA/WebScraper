﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Models.Factories
{
    public class HtmlLoaderFactory : IFactory<IHtmlLoader>
    {
        private readonly IServiceProvider _servicesProvider;

        public HtmlLoaderFactory(IServiceProvider serviceProvider)
        {
            _servicesProvider = serviceProvider;
        }

        public IHtmlLoader Get(SiteDto siteDto)
        {
            if (siteDto.Settings.UseSeleniumService)
                return _servicesProvider.GetService<SelenuimService>();
            else
                return _servicesProvider.GetService<HtmlLoader>();
        }
    }
}