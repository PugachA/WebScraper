﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WebScraper.WebApi.Helpers;

namespace WebScraper.WebApi.DTO
{
    public class SiteSettings
    {
        [Key]
        public int Id { get; set; }
        public bool AutoGenerateSchedule { get; set; }
        public bool UseSeleniumService { get; set; }

        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan MinCheckInterval { get; set; } = TimeSpan.FromMinutes(30);

        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(30);
    }
}