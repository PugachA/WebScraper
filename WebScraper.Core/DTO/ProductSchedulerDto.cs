using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebScraper.Core.DTO
{
    public class ProductSchedulerDto
    {
        [Range(0, int.MaxValue, ErrorMessage = "Id не может быть меньше 0")]
        public int ProductId { get; set; }

        [Required]
        public List<string> Scheduler { get; set; }
    }
}
