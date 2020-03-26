using System;
using System.Collections.Generic;
using System.Linq;
using WebScraper.WebApi.Models;

namespace WebScraper.WebApi.Cron
{
    public class BeruCronScheduler : ICronScheduler<Product>
    {
        private readonly SiteSettings _siteSettings;
        private readonly int _maxProductCount;

        public BeruCronScheduler(SiteSettings siteSettings)
        {
            this._siteSettings = siteSettings;
            _maxProductCount = (int)(TimeSpan.FromDays(1).Ticks / _siteSettings.MinCheckInterval.Ticks);
        }

        public Dictionary<Product, List<string>> GenerateSchedule(IEnumerable<Product> products)
        {
            if (products.Count() > _maxProductCount)
                throw new ArgumentOutOfRangeException($"Количество товаров {products.Count()} превысило максимально допустимое количество {_maxProductCount}");

            Dictionary<Product, List<string>> productCrons = new Dictionary<Product, List<string>>();
            DateTime dateTime = DateTime.Today;

            int count = 0;
            while (count < _maxProductCount)
            {
                foreach (Product product in products)
                {
                    if (productCrons.ContainsKey(product))
                        productCrons[product].Add(GenarateCronPattern(dateTime));
                    else
                        productCrons.Add(product, new List<string> { GenarateCronPattern(dateTime) });

                    dateTime = dateTime.Add(_siteSettings.CheckInterval);
                    count++;
                }
            }

            return productCrons;
        }

        private string GenarateCronPattern(DateTime dateTime) => $"{dateTime.Second} {dateTime.Minute} {dateTime.Hour} ? * * *";
    }
}
