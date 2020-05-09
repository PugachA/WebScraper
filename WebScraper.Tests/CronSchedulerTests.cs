using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.Json;
using WebScraper.WebApi.Cron;
using WebScraper.WebApi.DTO;
using WebScraper.WebApi.Models;

namespace WebScraper.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SampleTest1()
        {
            List<ProductDto> products = new List<ProductDto>
            {
                new ProductDto(),
                new ProductDto(),
                new ProductDto()
            };

            var siteSettings = new SiteSettings
            {
                CheckInterval = TimeSpan.FromMinutes(35)
            };

            var beruCronScheduler = new BeruCronScheduler(siteSettings);

            var dic = beruCronScheduler.GenerateSchedule(products);

            foreach (var keyValue in dic)
            {
                var t = JsonSerializer.Serialize(keyValue.Value);
                foreach (var cron in keyValue.Value)
                    Console.WriteLine($"{keyValue.Key} - {cron}");
            }
        }
    }
}