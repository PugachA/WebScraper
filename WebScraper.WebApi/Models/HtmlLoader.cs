using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebScraper.WebApi.Models
{
    public class HtmlLoader
    {
        private readonly Uri _baseUri;
        private readonly IRestClient _restClient;

        public HtmlLoader(Uri baseUri)
        {
            _baseUri = baseUri;
            _restClient = new RestClient(_baseUri);
        }

        public async Task<IHtmlDocument> Load(string prefix = "")
        {
            var source = await GetContent(prefix);

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(source);

            return document;
        }

        private async Task<string> GetContent(string prefix)
        {
            var request = new RestRequest(prefix);

            IRestResponse response = await _restClient.ExecuteGetAsync(request);

            if (!response.IsSuccessful)
            {
                //_logger.LogError($"Не удалось получить информацию от MvcPaySystemAdmins {response.ErrorMessage} {response.ErrorException}");
                return null;
            }

            return response.Content;
        }
    }
}
