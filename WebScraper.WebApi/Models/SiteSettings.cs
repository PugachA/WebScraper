using System;

namespace WebScraper.WebApi.Models
{
    public class SiteSettings
    {
        public string BaseUrl { get; set; } 
        public bool AutoGenerateSchedule { get; set; }
        public TimeSpan MinCheckInterval { get; set; } = TimeSpan.FromMinutes(30);
        public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(30);
    }
}