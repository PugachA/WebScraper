using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebScraper.Data.Models
{
    public class Site
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string BaseUrl { get; set; }

        [ForeignKey(nameof(SiteSettings))]
        [JsonIgnore]
        public int SettingsId { get; set; }

        public SiteSettings Settings { get; set; }

        [JsonIgnore]
        public List<Product> Products { get; set; }

        public Site(string name, SiteSettings settings)
        {
            Name = name;
            Settings = settings;
        }

        public Site()
        {
        }
    }
}
