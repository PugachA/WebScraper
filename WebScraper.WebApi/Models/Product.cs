using System.Collections.Generic;
using WebScraper.WebApi.Models.Interfaces;

namespace WebScraper.WebApi.Models
{
    public class Product : IProduct
    {
        public string Url { get; }

        public IProductProperties ProductProperties { get; set; }

        public Product(string url, IProductProperties productProperties)
        {
            Url = url;
            ProductProperties = productProperties;
        }

        public override bool Equals(object obj)
        {
            return obj is Product product &&
                   Url == product.Url;
        }

        public override string ToString()
        {
            return Url;
        }

        public override int GetHashCode()
        {
            return -1915121810 + EqualityComparer<string>.Default.GetHashCode(Url);
        }
    }
}