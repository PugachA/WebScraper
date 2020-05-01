using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebScraper.WebApi.Models
{
    public class HtmlLoader
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public HtmlLoader(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException($"Параметр {nameof(logger)} не может быть null");

            _logger = logger;
            _httpClient = new HttpClient();
        }

        public async Task<IHtmlDocument> Load(string requestUri)
        {
            var source = await GetContent(requestUri);

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(source);

            return document;
        }

        private async Task<string> GetContent(string requestUri)
        {
            var response = await _httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Не удалось отправить запрос по {requestUri}");
                response.EnsureSuccessStatusCode();
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
