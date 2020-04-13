using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebScraper.WebApi.DTO
{
    public class PriceDto
    {
        [Key]
        public int Id { get; set; }
        public decimal Price { get; set; }
        public decimal? DicountPrice { get; set; }
        public double? DiscountPercentage { get; set; }
        public DateTime Date { get; set; }

        [JsonIgnore]
        public ProductDto Product { get; set; }

        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
    }
}
