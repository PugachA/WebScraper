using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebScraper.WebApi.Models
{
    public class PriceInfo
    {
        public int Price { get; set; }
        public int? DicountPrice { get; set; }
        public int? DiscountPercentage { get; set; }

        public PriceInfo(int price, int? dicountPrice)
        {
            this.Price = price;
            this.DicountPrice = dicountPrice;
            this.DiscountPercentage = this.DicountPrice != null ? (this.Price - this.DicountPrice) / this.Price : null;
        }
    }
}
