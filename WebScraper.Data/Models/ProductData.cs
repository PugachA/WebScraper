using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebScraper.Data.Models
{
    public class ProductData
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Price { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? DiscountPrice { get; set; }
        public decimal? DiscountPercentage { get; private set; }
        
        [MaxLength(1024)]
        public string AdditionalInformation { get; set; }

        public DateTime Date { get; set; }

        [JsonIgnore]
        public Product Product { get; set; }

        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
    }
}
