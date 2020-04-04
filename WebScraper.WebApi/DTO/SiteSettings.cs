using System;
using System.ComponentModel.DataAnnotations;

namespace WebScraper.WebApi.DTO
{
    public class SiteSettings
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string BaseUrl { get; set; } 
        public bool AutoGenerateSchedule { get; set; }
        public TimeSpan MinCheckInterval { get; set; } = TimeSpan.FromMinutes(30);
        public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(30);
    }
}