
namespace Events.Application.Interfaces;

/// <summary>
/// Интерфейс кеша
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Добавить элемент в кеш как строку
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <param name="key">Ключ</param>
    /// <param name="value">Вставляемый объект</param>
    /// <param name="ttl">Время жизни в секундах</param>
    /// <returns>true - успешно, false - ошибка</returns>
    Task<bool> SetAsync<T>(string key, T value, TimeSpan ttl);

    /// <summary>
    /// Получить объект из кеша
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <param name="key">Ключ</param>
    /// <returns>Объект или null</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Удалить из кеша
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>true -успешно, false- ошибка</returns>
    Task<bool> DeleteAsync(string key);

}
