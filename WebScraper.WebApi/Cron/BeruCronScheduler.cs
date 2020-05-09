using System;
using System.Collections.Generic;
using System.Linq;
using WebScraper.WebApi.DTO;
using WebScraper.WebApi.Models;

namespace WebScraper.WebApi.Cron
{
    public class BeruCronScheduler : ICronScheduler<ProductDto>
    {
        private readonly SiteSettings _siteSettings;
        private readonly int _maxProductCount;
        private readonly TimeSpan _interval;

        public BeruCronScheduler(SiteSettings siteSettings)
        {
            this._siteSettings = siteSettings;

            _interval = _siteSettings.CheckInterval > _siteSettings.MinCheckInterval ? _siteSettings.CheckInterval : _siteSettings.MinCheckInterval;
            _maxProductCount = (int)(TimeSpan.FromDays(1).Ticks / _interval.Ticks);
        }

        public Dictionary<ProductDto, List<string>> GenerateSchedule(IEnumerable<ProductDto> products)
        {
            if (!products.Any())
                throw new ArgumentException($"{nameof(products)} не может быть пустым");

            if (products.Count() > _maxProductCount)
                throw new ArgumentOutOfRangeException($"Количество товаров {products.Count()} превысило максимально допустимое количество {_maxProductCount}");

            Dictionary<ProductDto, List<DateTime>> productTimes = new Dictionary<ProductDto, List<DateTime>>();
            DateTime dateTime = DateTime.Today;

            int count = 0;
            //TODO добавить проверку разницы между первым и последнем временем
            while (count < _maxProductCount)
            {
                foreach (ProductDto product in products)
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

        private Dictionary<ProductDto, List<string>> ConvertToCron(Dictionary<ProductDto, List<DateTime>> productTimes)
        {
            var productCronTime = new Dictionary<ProductDto, List<string>>();

            foreach (var productTime in productTimes)
                foreach (var time in productTime.Value)
                {
                    if (productCronTime.ContainsKey(productTime.Key))
                        productCronTime[productTime.Key].Add($"{time.Second} {time.Minute} {time.Hour} ? * * *");
                    else
                        productCronTime.Add(productTime.Key, new List<string> { $"{time.Second} {time.Minute} {time.Hour} ? * * *" });
                }

            return productCronTime;
        }
    }
}
