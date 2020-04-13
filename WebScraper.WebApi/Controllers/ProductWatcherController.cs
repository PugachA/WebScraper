using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraper.WebApi.DTO;
using WebScraper.WebApi.Models;

namespace WebScraper.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductWatcherController : ControllerBase
    {
        private readonly ProductWatcherContext _productWatcherContext;
        private readonly ProductWatcherManager _productWatcherManager;

        public ProductWatcherController(ProductWatcherContext productWatcherContext, ProductWatcherManager productWatcherManager)
        {
            _productWatcherContext = productWatcherContext ?? throw new ArgumentNullException($"Параметр {nameof(productWatcherContext)} не может быть null");
            _productWatcherManager = productWatcherManager ?? throw new ArgumentNullException($"Параметр {nameof(productWatcherManager)} не может быть null");
        }

        [HttpGet("price")]
        public async Task<ActionResult<PriceDto>> GetPrice(int productId)
        {
            try
            {
                if (productId < 0)
                {
                    //_logger.LogInformation($"Запрос не прошел валидацию. {nameof(paymentId)}={paymentId} должен быть неотрицательным числом");
                    return BadRequest($"Запрос не прошел валидацию. {nameof(productId)}={productId} должен быть неотрицательным числом");
                }

                var productDto = await _productWatcherManager.GetProductAsync(productId);

                //if (productDto == null)
                //_logger.LogError($"Не удалось найти цену для товара {nameof(productId)}={productId}");

                return Ok(productDto);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Внутренняя ошибка сервиса при обработке запроса");
                return StatusCode(500, "Ошибка при обработке запроса");
            }
        }

        [HttpPost("price")]
        public async Task<ActionResult> PostPrice(int productId)
        {
            try
            {
                if (productId < 0)
                {
                    //_logger.LogInformation($"Запрос не прошел валидацию. {nameof(paymentId)}={paymentId} должен быть неотрицательным числом");
                    return BadRequest($"Запрос не прошел валидацию. {nameof(productId)}={productId} должен быть неотрицательным числом");
                }

                await _productWatcherManager.ExtractPriceDto(productId);

                return Ok();
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Внутренняя ошибка сервиса при обработке запроса");
                return StatusCode(500, "Ошибка при обработке запроса");
            }
        }

        [HttpPost("product")]
        public async Task<ActionResult<ProductDto>> Post(ProductDto product)
        {
            try
            {
                return Ok(null);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Внутренняя ошибка сервиса при обработке запроса");
                return StatusCode(500, "Ошибка при обработке запроса");
            }
        }

    }
}
