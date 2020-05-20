using System.ComponentModel.DataAnnotations;

namespace WebScraper.WebApi.Models
{
    public class PriceInfo
    {
        public decimal? Price { get; set; }
        public decimal? DicountPrice { get; set; }
        public double? DiscountPercentage { get; set; }
        public string AdditionalInformation { get; set; }

        public PriceInfo()
        {
        }

        public PriceInfo(decimal? price, decimal? dicountPrice, string additionalInformation = "")
        {
            this.Price = price;
            this.DicountPrice = dicountPrice;
            this.DiscountPercentage = this.DicountPrice != null ? (double?)((this.Price - this.DicountPrice) / this.Price) : null;
            this.AdditionalInformation = additionalInformation;
        }
    }
}
