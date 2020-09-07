using System;
using System.Collections.Generic;
using System.Linq;
using WebScraper.Data.Models;

namespace WebScraper.Core.Cron
{
    public class CronSchedulerGenerator : ICronScheduler<Product>
    {
        private readonly SiteSettings _siteSettings;
        private readonly int _maxProductCount;
        private readonly TimeSpan _interval;

        public CronSchedulerGenerator(SiteSettings siteSettings)
        {
            this._siteSettings = siteSettings;

            _interval = _siteSettings.CheckInterval > _siteSettings.MinCheckInterval ? _siteSettings.CheckInterval : _siteSettings.MinCheckInterval;
            _maxProductCount = (int)(TimeSpan.FromDays(1).Ticks / _interval.Ticks);
        }

        public Dictionary<Product, List<string>> GenerateSchedule(IEnumerable<Product> products)
        {
            if (!products.Any())
                throw new ArgumentException($"{nameof(products)} не может быть пустым");

            if (products.Count() > _maxProductCount)
                throw new ArgumentOutOfRangeException($"Количество товаров {products.Count()} превысило максимально допустимое количество {_maxProductCount}");

            Dictionary<Product, List<DateTime>> productTimes = new Dictionary<Product, List<DateTime>>();
            DateTime dateTime = DateTime.Today;

            int count = 0;
            while (count < _maxProductCount)
            {
                foreach (Product product in products)
                {
                    if (productTimes.ContainsKey(product))
                        productTimes[product].Add(dateTime);
                    else
                        productTimes.Add(product, new List<DateTime> { dateTime });

                    dateTime = dateTime.Add(_interval);
                    count++;
                }
            }

            if (productTimes.First().Value.First() - productTimes.Last().Value.Last().AddDays(-1) < _interval)
                productTimes.Last().Value.RemoveAt(productTimes.Last().Value.Count - 1);

            return ConvertToCron(productTimes);
        }

        private Dictionary<Product, List<string>> ConvertToCron(Dictionary<Product, List<DateTime>> productTimes)
        {
            var productCronTime = new Dictionary<Product, List<string>>();

            foreach (var productTime in productTimes)
                foreach (var time in productTime.Value)
                {
                    if (productCronTime.ContainsKey(productTime.Key))
                        productCronTime[productTime.Key].Add($"{time.Second} {time.Minute} {time.Hour} ? * *");
                    else
                        productCronTime.Add(productTime.Key, new List<string> { $"{time.Second} {time.Minute} {time.Hour} ? * *" });
                }

            return productCronTime;
        }
    }
}
