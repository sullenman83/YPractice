
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
    /// <param name="token">Токен отмены</param>
    /// <returns>true - успешно, false - ошибка</returns>
    Task<bool> StringSetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken token = default);

    /// <summary>
    /// Получить объект из кеша
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    /// <param name="key">Ключ</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Объект или null</returns>
    Task<T?> StringGetAsync<T>(string key, CancellationToken token = default);

    /// <summary>
    /// Удалить из кеша
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>true -успешно, false- ошибка</returns>
    Task<bool> Delete(string key, CancellationToken token = default);

}
