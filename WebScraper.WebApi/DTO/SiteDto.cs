using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebScraper.WebApi.DTO
{
    public class SiteDto
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public SiteSettings Settings { get; set; }

        public List<ProductDto> Products { get; set; }

        public SiteDto(string name, SiteSettings settings)
        {
            Name = name;
            Settings = settings;
        }

        public SiteDto()
        {
        }
    }
}
