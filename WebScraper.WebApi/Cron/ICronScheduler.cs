using System.Collections.Generic;
using WebScraper.WebApi.Models;

namespace WebScraper.WebApi.Cron
{
    public interface ICronScheduler<IProduct>
    {
        Dictionary<Product, List<string>> GenerateSchedule(IEnumerable<Product> products);
    }
}
