using System.Collections.Generic;
using WebScraper.Data.Models;

namespace WebScraper.Core.Cron
{
    public interface ICronScheduler<IProduct>
    {
        Dictionary<Product, List<string>> GenerateSchedule(IEnumerable<Product> products);
    }
}
