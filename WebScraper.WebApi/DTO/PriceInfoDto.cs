namespace WebScraper.WebApi.DTO
{
    public class PriceInfoDto
    {
        public int Price { get; set; }
        public int? DicountPrice { get; set; }
        public double? DiscountPercentage { get; set; }

        public PriceInfoDto(int price, int? dicountPrice)
        {
            this.Price = price;
            this.DicountPrice = dicountPrice;
            this.DiscountPercentage = this.DicountPrice != null ? (double?)(this.Price - this.DicountPrice) / this.Price : null;
        }
    }
}
