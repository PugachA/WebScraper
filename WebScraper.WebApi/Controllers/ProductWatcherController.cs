using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebScraper.WebApi.DTO;
using WebScraper.WebApi.Models;

namespace WebScraper.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductWatcherController : ControllerBase
    {
        private readonly ProductWatcherManager _productWatcherManager;
        private readonly ILogger<ProductWatcherController> _logger;

        public ProductWatcherController(ProductWatcherManager productWatcherManager, ILogger<ProductWatcherController> logger)
        {
            _productWatcherManager = productWatcherManager ?? throw new ArgumentNullException($"Параметр {nameof(productWatcherManager)} не может быть null");
            _logger = logger;
        }

        [HttpGet("price")]
        public async Task<ActionResult<PriceDto>> GetPrice(int productId)
        {
            try
            {
                if (productId < 0)
                {
                    _logger.LogInformation($"Запрос не прошел валидацию. {nameof(productId)}={productId} должен быть неотрицательным числом");
                    return BadRequest($"Запрос не прошел валидацию. {nameof(productId)}={productId} должен быть неотрицательным числом");
                }

                var priceDto = await _productWatcherManager.GetLastPriceDto(productId);

                if (priceDto == null)
                {
                    _logger.LogError($"Не удалось найти цену для товара {nameof(productId)}={productId}");
                    return NoContent();
                }

                return Ok(priceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Внутренняя ошибка сервиса при обработке запроса");
                return StatusCode(500, "Ошибка при обработке запроса");
            }
        }

        [HttpPost("price")]
        public async Task<ActionResult> PostPrice(int productId)
        {
            try
            {
                _logger.LogInformation($"Поступил запрос на поиск цены по {nameof(productId)}={productId}");

                if (productId < 0)
                {
                    _logger.LogError($"Запрос не прошел валидацию. {nameof(productId)}={productId} должен быть неотрицательным числом");
                    return BadRequest($"Запрос не прошел валидацию. {nameof(productId)}={productId} должен быть неотрицательным числом");
                }

                var productDto = await _productWatcherManager.GetProductAsync(productId);
                _logger.LogInformation($"Найден продукт по {nameof(productId)}={productId} {JsonSerializer.Serialize(productDto)}");

                if (productDto == null)
                {
                    _logger.LogError($"Не удалось найти продукт по id={productId}");
                    return NoContent();
                }

                await _productWatcherManager.ExtractPriceDto(productDto);
                _logger.LogInformation($"Цена для {nameof(productId)}={productId} успешно найдена и добавлена в БД");

                return Ok($"Цена для {nameof(productId)}={productId} успешно найдена и добавлена в БД");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Внутренняя ошибка сервиса при обработке запроса");
                return StatusCode(500, "Ошибка при обработке запроса");
            }
        }

        [HttpPut("product")]
        public async Task<ActionResult<ProductDto>> Put(ProductDto product)
        {
            try
            {
                
                return Ok(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Внутренняя ошибка сервиса при обработке запроса");
                return StatusCode(500, "Ошибка при обработке запроса");
            }
        }

    }
}
