using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebScraper.WebApi.DTO
{
    public class CreateProductDto
    {
        [Url(ErrorMessage = "Неверный формат url")]
        public string ProductUrl { get; set; }

        public List<string> Scheduler { get; set; }
    }
}
