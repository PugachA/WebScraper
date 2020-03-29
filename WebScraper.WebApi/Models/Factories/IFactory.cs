
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
        /// <param name="key"></param>
        /// <returns></returns>
        T Get(Site site);
    }
}
