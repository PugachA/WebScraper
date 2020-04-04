using System;
using System.Collections.Generic;
using WebScraper.WebApi.DTO.Interfaces;

namespace WebScraper.WebApi.DTO
{
    public class ProductDto : IProduct
    {
        public Uri Url { get; }
        public SiteDto Site { get; set; }
        public List<string> Scheduler { get; set; }

        public ProductDto(Uri url, SiteDto site, List<string> scheduler)
        {
            Url = url;
            Site = site;
            Scheduler = scheduler;
        }
    }
}