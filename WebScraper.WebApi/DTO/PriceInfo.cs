namespace WebScraper.WebApi.DTO
{
    public class PriceInfo
    {
        public decimal Price { get; set; }
        public decimal? DicountPrice { get; set; }
        public double? DiscountPercentage { get; set; }

        public PriceInfo(decimal price, decimal? dicountPrice)
        {
            this.Price = price;
            this.DicountPrice = dicountPrice;
            this.DiscountPercentage = this.DicountPrice != null ? (double?)((this.Price - this.DicountPrice) / this.Price) : null;
        }
    }
}
