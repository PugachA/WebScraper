using System;
using System.Collections.Generic;

namespace WebScraper.WebApi.DTO.Interfaces
{
    public interface IProduct
    {
        List<string> Scheduler { get; set; }
        SiteDto Site { get; set; }
        string Url { get; }
    }
}