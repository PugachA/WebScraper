using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Helpers
{
    public class HangfireSchedulerClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HangfireSchedulerClient> _logger;
        private readonly string _baseUrl;

        public HangfireSchedulerClient(IConfiguration configuration, ILogger<HangfireSchedulerClient> logger)
        {
            if (configuration == null)
                throw new ArgumentNullException($"Значение {nameof(configuration)} не может быть null");

            if (logger == null)
                throw new ArgumentNullException($"Значение {nameof(logger)} не может быть null");

            _logger = logger;
            _baseUrl = configuration.GetValue<string>("HangfireSchedulerBaseUrl");
            _httpClient = new HttpClient();
        }

        public async Task CreateOrUpdateScheduler(ProductSchedulerDto productSchedulerDto)
        {
            if(productSchedulerDto == null)
            {
                _logger.LogError($"Параметр {nameof(productSchedulerDto)} не может быть null");
                throw new ArgumentNullException($"Параметр {nameof(productSchedulerDto)} не может быть null");
            }

            var content = new StringContent(JsonSerializer.Serialize(productSchedulerDto), Encoding.UTF8, "application/json");

            var requestUrl = $"{_baseUrl}/api/HangfireScheduler/Products";
            var response = await _httpClient.PostAsync(requestUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Не удалось отправить запрос по {requestUrl}");
                response.EnsureSuccessStatusCode();
            }

            _logger.LogInformation($"Успешно отправлен запрос на создание или обновления расписания {requestUrl}");
        }

        public async Task DeleteProductScheduler(int productId)
        {
            var requestUrl = $"{_baseUrl}/api/HangfireScheduler/Products?productId={productId}";
            var response = await _httpClient.DeleteAsync(requestUrl);

            if (response.StatusCode == HttpStatusCode.NotFound)
                _logger.LogWarning($"Не найдена расписания для удаления для {nameof(productId)}={productId}");
            else
            {
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Не удалось отправить запрос по {requestUrl}");
                    response.EnsureSuccessStatusCode();
                }

                _logger.LogInformation($"Успешно отправлен запрос на удаление {requestUrl}");
            }
        }

        public async Task DeleteScheduler(string recurringJobId)
        {
            if (String.IsNullOrEmpty(recurringJobId))
            {
                _logger.LogError($"Параметр {nameof(recurringJobId)} не может быть null или empty");
                throw new ArgumentNullException($"Параметр {nameof(recurringJobId)} не может быть null или empty");
            }

            var requestUrl = $"{_baseUrl}/api/HangfireScheduler?recurringJobId={recurringJobId}";
            var response = await _httpClient.DeleteAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Не удалось отправить запрос по {requestUrl}");
                response.EnsureSuccessStatusCode();
            }

            _logger.LogInformation($"Успешно отправлен запрос {requestUrl}");
        }

    }
}
