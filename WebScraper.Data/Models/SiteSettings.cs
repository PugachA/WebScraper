using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WebScraper.Data.Helpers;

namespace WebScraper.Data.Models
{
    public class SiteSettings
    {
        [Key]
        public int Id { get; set; }
        public bool AutoGenerateSchedule { get; set; }

        [Required]
        [MaxLength(50)]
        public string HtmlLoader { get; set; }

        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan MinCheckInterval { get; set; } = TimeSpan.FromMinutes(30);

        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(30);
    }
}