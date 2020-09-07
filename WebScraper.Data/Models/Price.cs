using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebScraper.Data.Models
{
    public class Price
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [Column("Price", TypeName = "decimal(18, 2)")]
        public decimal? PriceValue { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? DicountPrice { get; set; }
        public double? DiscountPercentage { get; set; }
        
        [MaxLength(1024)]
        public string AdditionalInformation { get; set; }

        public DateTime Date { get; set; }

        [JsonIgnore]
        public Product Product { get; set; }

        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
    }
}
