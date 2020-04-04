namespace WebScraper.WebApi.DTO
{
    public class Site
    {
        public string Name { get; set; }
        public SiteSettings Settings { get; set; }

        public Site(string name, SiteSettings settings)
        {
            Name = name;
            Settings = settings;
        }
    }
}
