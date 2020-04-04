using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebScraper.WebApi.DTO
{
    public class PriceDto
    {
        [Key]
        public int Id { get; set; }
        public int Price { get; set; }
        public int? DicountPrice { get; set; }
        public double? DiscountPercentage { get; set; }
        public DateTime Date { get; set; }

        public ProductDto Product { get; set; }

        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
    }
}
