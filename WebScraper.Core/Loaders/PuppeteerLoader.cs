using AngleSharp.Html.Dom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Data.Models;

namespace WebScraper.Core.Loaders
{
    public class PuppeteerLoader : IHtmlLoader
    {
        private readonly ILogger<PuppeteerLoader> logger;
        private readonly IConfiguration configuration;

        public PuppeteerLoader(ILogger<PuppeteerLoader> logger)
        {
            this.logger = logger;
        }

        public Task<IHtmlDocument> Load(string requestUri, Site siteDto, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
