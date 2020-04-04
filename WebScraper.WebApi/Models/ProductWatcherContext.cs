using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Models
{
    public class ProductWatcherContext: DbContext
    {
        public DbSet<ProductDto> Products { get; set; }
        public DbSet<SiteDto> Sites { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }
        public DbSet<PriceInfoDto> Prices { get; set; }

        public ProductWatcherContext(DbContextOptions<ProductWatcherContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
