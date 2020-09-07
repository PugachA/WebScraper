using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        public DbSet<Price> Prices { get; set; }

        public ProductWatcherContext(DbContextOptions<ProductWatcherContext> options) : base(options)
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
        }
    }
}
