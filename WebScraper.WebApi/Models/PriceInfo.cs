using System;
using System.ComponentModel.DataAnnotations;
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Models
{
    public class PriceInfo
    {
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public decimal? DicountPrice { get; set; }
        public double? DiscountPercentage { get; set; }
        public string AdditionalInformation { get; set; }

        public PriceInfo()
        {
        }

        public PriceInfo(decimal? price, decimal? dicountPrice, string name, string additionalInformation = null)
        {
            this.Name = name;
            this.Price = price;
            this.DicountPrice = dicountPrice;
            this.DiscountPercentage = this.DicountPrice != null ? (double?)((this.Price - this.DicountPrice) / this.Price) : null;
            this.AdditionalInformation = additionalInformation;
        }

        public PriceDto ConvertToPriceDto(int productId)
        {
            var priceDto = new PriceDto
            {
                Name = Name,
                Price = Price,
                DicountPrice = DicountPrice,
                DiscountPercentage = DiscountPercentage,
                AdditionalInformation = AdditionalInformation,
                Date = DateTime.Now,
                ProductId = productId
            };

            return priceDto;
        }
    }
}
