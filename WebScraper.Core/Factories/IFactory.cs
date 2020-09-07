
using WebScraper.Data.Models;

namespace WebScraper.Core.Factories
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
        T Get(Site site);
    }
}
