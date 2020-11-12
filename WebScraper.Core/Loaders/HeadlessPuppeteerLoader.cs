using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebScraper.Core.Loaders
{
    public class HeadlessPuppeteerLoader : PuppeteerLoader
    {
        public HeadlessPuppeteerLoader(IConfiguration configuration, ILogger<PuppeteerLoader> logger) : base(configuration, logger, true)
        {
        }
    }
}
