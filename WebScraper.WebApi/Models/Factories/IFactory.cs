﻿
using WebScraper.WebApi.DTO;

namespace WebScraper.WebApi.Models.Factories
{
    /// <summary>
    /// Интерфейс для создания фабрик
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFactory<T>
    {
        /// <summary>
        /// Получение значения по ключу
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        T Get(SiteDto site);
    }
}
