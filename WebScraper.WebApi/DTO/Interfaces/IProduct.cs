using System;
using System.Collections.Generic;

namespace WebScraper.WebApi.DTO.Interfaces
{
    public interface IProduct
    {
        List<string> Scheduler { get; set; }
        Site Site { get; set; }
        Uri Url { get; }
    }
}