namespace WebScraper.WebApi.DTO
{
    public class PriceInfo
    {
        public int Price { get; set; }
        public int? DicountPrice { get; set; }
        public double? DiscountPercentage { get; set; }

        public PriceInfo(int price, int? dicountPrice)
        {
            this.Price = price;
            this.DicountPrice = dicountPrice;
            this.DiscountPercentage = this.DicountPrice != null ? (double?)(this.Price - this.DicountPrice) / this.Price : null;
        }
    }
}
