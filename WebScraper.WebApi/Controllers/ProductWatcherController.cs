﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebScraper.Core;
using WebScraper.Core.DTO;
using WebScraper.Core.Helpers;
using WebScraper.Data.Models;

namespace WebScraper.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductWatcherController : ControllerBase
    {
        private readonly ProductWatcherManager _productWatcherManager;
        private readonly ILogger<ProductWatcherController> _logger;
        private readonly HangfireSchedulerClient hangfireSchedulerClient;
        private readonly IConfiguration configuration;

        public ProductWatcherController(ProductWatcherManager productWatcherManager, ILogger<ProductWatcherController> logger, HangfireSchedulerClient hangfireSchedulerClient)
        {
            _productWatcherManager = productWatcherManager ?? throw new ArgumentNullException($"Параметр {nameof(productWatcherManager)} не может быть null");
            _logger = logger;
            this.hangfireSchedulerClient = hangfireSchedulerClient;
        }

        [HttpGet("price")]
        public async Task<ActionResult<ProductData>> GetPrice(int productId)
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

        [HttpGet("cvprice")]
        public async Task<ActionResult<ProductData>> GetCVPriceInfo(string productUri)
        {
            try
            {
                if (string.IsNullOrEmpty(productUri))
                {
                    _logger.LogInformation($"Validation failed. Parametr {nameof(productUri)} can not be null or empty");
                    return BadRequest($"Validation failed. Parametr {nameof(productUri)} can not be null or empty");
                }

                var priceInfo = await _productWatcherManager.GetCVProductData(productUri);

                if (priceInfo == null)
                {
                    _logger.LogError($"Can not found price info for product {nameof(productUri)}={productUri}");
                    return NoContent();
                }

                return Ok(priceInfo);
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
                    return NotFound();
                }

                if (productDto.IsDeleted)
                {
                    _logger.LogError("Нельзя запрашивать цену по удаленную продукту");
                    return BadRequest("Нельзя запрашивать цену по удаленную продукту");
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

        [HttpGet("product")]
        public async Task<ActionResult<Product>> GetProduct(int productId)
        {
            try
            {
                if (productId < 0)
                {
                    _logger.LogError($"Запрос не прошел валидацию. {nameof(productId)}={productId} должен быть неотрицательным числом");
                    return BadRequest($"Запрос не прошел валидацию. {nameof(productId)}={productId} должен быть неотрицательным числом");
                }

                var productDto = await _productWatcherManager.GetProductAsync(productId);

                if (productDto == null)
                {
                    _logger.LogError($"Не удалось найти продукт с {nameof(productId)}={productId}");
                    return NotFound($"Не удалось найти продукт с {nameof(productId)}={productId}");
                }

                return Ok(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Внутренняя ошибка сервиса при обработке запроса");
                return StatusCode(500, "Ошибка при обработке запроса");
            }
        }

        [HttpPost("product")]
        public async Task<ActionResult<Product>> CreateProduct(CreateProductDto createProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError($"Валидация модели не успешна {ModelState}");
                    return BadRequest(ModelState);
                }

                //Детектируем сайт
                if (!Uri.TryCreate(createProductDto.ProductUrl, UriKind.Absolute, out Uri productUri))
                {
                    _logger.LogError($"Не удалось преобразовать {nameof(createProductDto.ProductUrl)}={createProductDto.ProductUrl} в {typeof(Uri)}");
                    return BadRequest($"Не удалось преобразовать {nameof(createProductDto.ProductUrl)}={createProductDto.ProductUrl} в {typeof(Uri)}");
                }

                var siteDto = await _productWatcherManager.GetSiteByProductUrl(productUri);

                if (siteDto == null)
                {
                    _logger.LogError($"Не удалось детектировать сайт по url={productUri.AbsoluteUri}");
                    return NotFound($"Не удалось детектировать сайт по url={productUri.AbsoluteUri}");
                }

                Product productDto = null;

                if (siteDto.Settings.AutoGenerateSchedule)
                {
                    if (createProductDto.Scheduler != null)
                    {
                        _logger.LogError($"Нельзя задавать строгое расписание {nameof(siteDto.Settings.AutoGenerateSchedule)}={siteDto.Settings.AutoGenerateSchedule}");
                        return BadRequest($"Нельзя задавать строгое расписание {nameof(siteDto.Settings.AutoGenerateSchedule)}={siteDto.Settings.AutoGenerateSchedule}");
                    }

                    productDto = await _productWatcherManager.UpdateProductAutogenerateScheduler(createProductDto.ProductUrl, siteDto);
                }
                else
                {
                    if (createProductDto.Scheduler == null || !createProductDto.Scheduler.Any())
                    {
                        _logger.LogError("Нельзя создавать пустой расписание");
                        return BadRequest("Нельзя создавать пустой расписание");
                    }

                    productDto = await _productWatcherManager.CreateProduct(createProductDto.ProductUrl, siteDto, createProductDto.Scheduler, true);
                }

                return CreatedAtAction(
                    nameof(GetProduct),
                    new { id = productDto.Id },
                    productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Внутренняя ошибка сервиса при обработке запроса");
                return StatusCode(500, "Ошибка при обработке запроса");
            }
        }

        [HttpDelete("product")]
        public async Task<ActionResult<Product>> DeleteProduct(int productId)
        {
            try
            {
                if (productId < 0)
                {
                    _logger.LogError($"Запрос не прошел валидацию. {nameof(productId)}={productId} должен быть неотрицательным числом");
                    return BadRequest($"Запрос не прошел валидацию. {nameof(productId)}={productId} должен быть неотрицательным числом");
                }

                var productDto = await _productWatcherManager.GetProductAsync(productId);

                if (productDto == null)
                {
                    _logger.LogError($"Не удалось найти продукт с {nameof(productId)}={productId}");
                    return NotFound($"Не удалось найти продукт с {nameof(productId)}={productId}");
                }

                if (productDto.IsDeleted)
                {
                    _logger.LogInformation($"Продукт с {nameof(productId)}={productId} уже был удален ранее");
                    return Ok($"Продукт с {nameof(productId)}={productId} уже был удален ранее");
                }

                await _productWatcherManager.SmartDelete(productDto);
                _logger.LogInformation($"Продукт с {nameof(productId)}={productId} помечен как удаленный");

                return Ok(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Внутренняя ошибка сервиса при обработке запроса");
                return StatusCode(500, "Ошибка при обработке запроса");
            }
        }

        [HttpPut(nameof(UpdateProductScheduler))]
        public async Task<ActionResult<Product>> UpdateProductScheduler(ProductSchedulerDto productSchedulerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError($"Валидация модели не успешна {ModelState}");
                    return BadRequest(ModelState);
                }

                var productDto = await _productWatcherManager.GetProductAsync(productSchedulerDto.ProductId);

                if (productDto == null)
                {
                    _logger.LogError($"Не удалось найти продукт с {nameof(productSchedulerDto.ProductId)}={productSchedulerDto.ProductId}");
                    return NotFound($"Не удалось найти продукт с {nameof(productSchedulerDto.ProductId)}={productSchedulerDto.ProductId}");
                }

                if (productDto.IsDeleted)
                {
                    _logger.LogError("Нельзя обновить расписание по удаленному продукту");
                    return BadRequest("Нельзя обновить расписание по удаленному продукту");
                }

                if (productDto.Site.Settings.AutoGenerateSchedule)
                {
                    _logger.LogError("Нельзя обновить расписание по продукту с автоматическим расписанием");
                    return BadRequest("Нельзя обновить расписание по продукту с автоматическим расписанием");
                }

                await _productWatcherManager.UpdateProductScheduler(productDto, productSchedulerDto.Scheduler);
                _logger.LogInformation($"Продукту с {nameof(productDto.Id)}={productDto.Id} изменено расписание на {JsonSerializer.Serialize(productSchedulerDto.Scheduler)}");

                return Ok(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Внутренняя ошибка сервиса при обработке запроса");
                return StatusCode(500, "Ошибка при обработке запроса");
            }
        }

        [HttpPut(nameof(UpdateSiteScheduler))]
        public async Task<IActionResult> UpdateSiteScheduler(int siteId)
        {
            try
            {
                if (siteId < 0)
                {
                    _logger.LogError($"Запрос не прошел валидацию. {nameof(siteId)}={siteId} должен быть неотрицательным числом");
                    return BadRequest($"Запрос не прошел валидацию. {nameof(siteId)}={siteId} должен быть неотрицательным числом");
                }

                var siteDto = await _productWatcherManager.GetSite(siteId);

                if (siteDto == null)
                {
                    _logger.LogError($"Не удалось найти сайт с {nameof(siteId)}={siteId}");
                    return NotFound($"Не удалось найти сайт с {nameof(siteId)}={siteId}");
                }

                if (!siteDto.Settings.AutoGenerateSchedule)
                {
                    _logger.LogError("Нельзя обновить расписание по сайту с не автоматическим расписанием");
                    return BadRequest("Нельзя обновить расписание по сайту с не автоматическим расписанием");
                }

                await _productWatcherManager.UpdateSiteScheduler(siteDto);
                _logger.LogInformation($"Расписание по сайту с {nameof(siteId)}={siteId} успешно обновлено");

                return Ok($"Расписание по сайту с {nameof(siteId)}={siteId} успешно обновлено");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Внутренняя ошибка сервиса при обработке запроса");
                return StatusCode(500, "Ошибка при обработке запроса");
            }
        }
    }
}
