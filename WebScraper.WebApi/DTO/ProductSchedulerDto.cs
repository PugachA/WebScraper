using System.Collections.Generic;

namespace WebScraper.WebApi.DTO
{
    public class ProductSchedulerDto
    {
        public int ProductId { get; set; }
        public List<string> Scheduler { get; set; }
    }
}
