using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WebScraper.Data.Models;

namespace WebScraper.Data
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            var siteSettings = new SiteSettings
            {
                Id = 1,
                AutoGenerateSchedule = false,
                HtmlLoader="SeleniumLoader",
                MinCheckInterval = new TimeSpan(0, 1, 0),
                CheckInterval = new TimeSpan(0, 1, 0)
            };

            var site = new Site
            {
                Id = 1,
                BaseUrl = "https://aliexpress.ru/",
                Name = "AliExpress",
                SettingsId = siteSettings.Id
            };

            var product = new Product
            {
                Id = 1,
                Url = "https://aliexpress.ru/item/4000561177801.html?spm=a2g0o.productlist.0.0.4c866c27X06Ss3&algo_pvid=e3004186-674b-47b9-9bfe-bde68786bf6c&algo_expid=e3004186-674b-47b9-9bfe-bde68786bf6c-1&btsid=0b8b15ea15903376264785085ed804&ws_ab_test=searchweb0_0,searchweb201602_,searchweb201603_",
                SiteId = site.Id,
                Scheduler = new List<string> { "0 */30 * ? * *" },
                IsDeleted = false
            };

            modelBuilder.Entity<SiteSettings>()
                .HasData(siteSettings);

            modelBuilder.Entity<Site>()
                .HasData(site);

            modelBuilder.Entity<Product>()
                .HasData(product);
        }
    }
}
