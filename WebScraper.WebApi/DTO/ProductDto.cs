using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using WebScraper.WebApi.DTO.Interfaces;

namespace WebScraper.WebApi.DTO
{
    public class ProductDto : IProduct
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public SiteDto Site { get; set; }

        [Required]
        public List<string> Scheduler { get; set; }

        public List<PriceDto> Prices { get; set; }

        public ProductDto(string url, SiteDto site, List<string> scheduler)
        {
            Url = url;
            Site = site;
            Scheduler = scheduler;
        }

        public ProductDto() { }
    }
}