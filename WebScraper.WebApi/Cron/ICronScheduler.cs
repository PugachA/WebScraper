using System.Collections.Generic;
using WebScraper.WebApi.DTO;
using WebScraper.WebApi.Models;

namespace WebScraper.WebApi.Cron
{
    public interface ICronScheduler<IProduct>
    {
        Dictionary<ProductDto, List<string>> GenerateSchedule(IEnumerable<ProductDto> products);
    }
}
