using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebScraper.Data.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public Site Site { get; set; }

        [Required]
        public List<string> Scheduler { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public List<Price> Prices { get; set; }

        public Product(string url, Site site, List<string> scheduler)
        {
            Url = url;
            Site = site;
            Scheduler = scheduler;
        }

        public Product() { }
    }
}