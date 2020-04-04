using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Models
{
    public class ProductWatcherContext : DbContext
    {
        public DbSet<ProductDto> Products { get; set; }
        public DbSet<SiteDto> Sites { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }
        public DbSet<PriceDto> Prices { get; set; }

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

            modelBuilder.Entity<ProductDto>()
            .Property(p => p.Scheduler)
            .HasConversion(
                v => JsonSerializer.Serialize(v, null),
                v => JsonSerializer.Deserialize<List<string>>(v, null));
        }
    }
}
