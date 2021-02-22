using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text.Json;
using WebScraper.Data.Models;

namespace WebScraper.Data
{
    public class ProductWatcherContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }
        public DbSet<ProductData> ProductData { get; set; }

        public ProductWatcherContext(DbContextOptions<ProductWatcherContext> options) : base(options)
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SiteSettings>()
                .Property(s => s.PriceParser)
                .HasDefaultValue("HtmlPriceParser");

            modelBuilder.Entity<SiteSettings>()
                .Property(s => s.HtmlLoader)
                .HasDefaultValue("HttpLoader");

            modelBuilder.Entity<SiteSettings>()
                .Property(s => s.MinCheckInterval)
                .HasConversion(new TimeSpanToStringConverter()); // or TimeSpanToTicksConverter

            modelBuilder.Entity<SiteSettings>()
                .Property(s => s.CheckInterval)
                .HasConversion(new TimeSpanToStringConverter()); // or TimeSpanToTicksConverter

            modelBuilder.Entity<Product>()
                .Property(p => p.Scheduler)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, null),
                    v => JsonSerializer.Deserialize<List<string>>(v, null));

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.Url, p.IsDeleted })
                .HasFilter("[IsDeleted] = 0")
                .IsUnique();

            //TODO: Add stored: true after migration to net5.0 
            modelBuilder.Entity<ProductData>()
                .Property(p => p.DiscountPercentage)
                .HasComputedColumnSql($"CONVERT(DECIMAL(18, 2), 100*([{nameof(Models.ProductData.Price)}]-[{nameof(Models.ProductData.DiscountPrice)}])/[{nameof(Models.ProductData.Price)}]");

            modelBuilder.Seed();
        }
    }
}
