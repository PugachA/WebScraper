using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebScraper.WebApi.DTO
{
    public class SiteDto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string BaseUrl { get; set; }

        public SiteSettings Settings { get; set; }

        [JsonIgnore]
        public List<ProductDto> Products { get; set; }

        public SiteDto(string name, SiteSettings settings)
        {
            Name = name;
            Settings = settings;
        }

        public SiteDto()
        {
        }
    }
}
