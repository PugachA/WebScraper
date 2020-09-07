using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.Json;
using WebScraper.WebApi.Cron;
using WebScraper.Data.Models;
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
            List<Product> products = new List<Product>
            {
                new Product(),
                new Product(),
            };

            var siteSettings = new SiteSettings
            {
                CheckInterval = TimeSpan.FromMinutes(35)
            };

            var beruCronScheduler = new CronSchedulerGenerator(siteSettings);

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