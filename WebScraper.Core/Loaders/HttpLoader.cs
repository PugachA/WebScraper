using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Data.Models;

namespace WebScraper.Core.Loaders
{
    public class HttpLoader : IHtmlLoader
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpLoader> _logger;

        public HttpLoader(ILogger<HttpLoader> logger)
        {
            if (logger == null)
                throw new ArgumentNullException($"Параметр {nameof(logger)} не может быть null");

            _logger = logger;

            //var proxy = new WebProxy
            //{
            //    Address = new Uri($"http://87.236.212.148:8795"),
            //    BypassProxyOnLocal = true,
            //    UseDefaultCredentials = true
            //};

            //// Now create a client handler which uses that proxy
            //var httpClientHandler = new HttpClientHandler
            //{
            //    Proxy = proxy,
            //};

            _httpClient = new HttpClient();
        }

        public async Task<IHtmlDocument> Load(string requestUri, Site siteDto, CancellationToken token)
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

            _logger.LogInformation($"Успешно отправлен запрос {requestUri}");

            return await response.Content.ReadAsStringAsync();
        }
    }
}
