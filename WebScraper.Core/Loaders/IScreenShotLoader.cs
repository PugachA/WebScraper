using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Data.Models;

namespace WebScraper.Core.Loaders
{
    public interface IScreenshotLoader
    {
        Task LoadScreenshot(string outputPath, string requestUri, Site site, CancellationToken token);
    }
}
