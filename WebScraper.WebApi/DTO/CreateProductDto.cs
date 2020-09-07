using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebScraper.WebApi.DTO
{
    public class CreateProductDto
    {
        [Url(ErrorMessage = "Неверный формат url")]
        public string ProductUrl { get; set; }

        public List<string> Scheduler { get; set; }
    }
}
