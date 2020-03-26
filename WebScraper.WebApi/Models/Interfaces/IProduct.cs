namespace WebScraper.WebApi.Models.Interfaces
{
    public interface IProduct
    {
        string Url { get; }
        IProductProperties ProductProperties { get; set; }

        int GetHashCode();
    }
}