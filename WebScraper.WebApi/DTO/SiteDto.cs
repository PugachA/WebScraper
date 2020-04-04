namespace WebScraper.WebApi.DTO
{
    public class SiteDto
    {
        public string Name { get; set; }
        public SiteSettings Settings { get; set; }

        public SiteDto(string name, SiteSettings settings)
        {
            Name = name;
            Settings = settings;
        }
    }
}
