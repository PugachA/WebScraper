﻿using System;
using System.Collections.Generic;
using WebScraper.WebApi.Models.Interfaces;

namespace WebScraper.WebApi.Models
{
    public class Product : IProduct
    {
        public Uri Url { get; }
        public Site Site { get; set; }
        public List<string> Scheduler { get; set; }

        public Product(Uri url, Site site, List<string> scheduler)
        {
            Url = url;
            Site = site;
            Scheduler = scheduler;
        }
    }
}