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
        public ProductWatcherController(ProductWatcherContext productWatcherContext)
        {
            _productWatcherContext = productWatcherContext;
        }

        [HttpGet("priceinfo")]
        public async Task<ActionResult<PriceInfo>> GetPriceInfo(int productId)
        {
            try
            {
                _productWatcherContext.SiteSettings.Add(new SiteSettings { AutoGenerateSchedule = true });
                _productWatcherContext.SaveChanges();

                var settings = _productWatcherContext.SiteSettings.First();

                if (productId < 0)
                {
                    //_logger.LogInformation($"Запрос не прошел валидацию. {nameof(paymentId)}={paymentId} должен быть неотрицательным числом");
                    return BadRequest($"Запрос не прошел валидацию. {nameof(productId)}={productId} должен быть неотрицательным числом");
                }

                return Ok(null);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Внутренняя ошибка сервиса при обработке запроса");
                return StatusCode(500, "Ошибка при обработке запроса");
            }
        }


        [HttpPost]
        public async Task<ActionResult> PostPriceInfo(int productId)
        {
            try
            {
                if (productId < 0)
                {
                    //_logger.LogInformation($"Запрос не прошел валидацию. {nameof(paymentId)}={paymentId} должен быть неотрицательным числом");
                    return BadRequest($"Запрос не прошел валидацию. {nameof(productId)}={productId} должен быть неотрицательным числом");
                }

                return Ok(null);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Внутренняя ошибка сервиса при обработке запроса");
                return StatusCode(500, "Ошибка при обработке запроса");
            }
        }

        [HttpPost]
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
