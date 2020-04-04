using NUnit.Framework;
using System;
using System.Collections.Generic;
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
            List<ProductDto> products = new List<ProductDto>();
            //{
            //    new Product("test1"),
            //    new Product("test2"),
            //    new Product("test3")
            //};

            var siteSettings = new SiteSettings
            {
                CheckInterval = TimeSpan.FromMinutes(30)
            };

            var beruCronScheduler = new BeruCronScheduler(siteSettings);

            var dic = beruCronScheduler.GenerateSchedule(products);

            foreach (var keyValue in dic)
            {
                foreach (var cron in keyValue.Value)
                    Console.WriteLine($"{keyValue.Key} - {cron}");
            }
        }
    }
}