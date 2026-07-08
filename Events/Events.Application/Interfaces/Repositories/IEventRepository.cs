using Events.Application.Models;
using Events.Application.Models.FilterModels;
using Events.Domain.Models;

namespace Events.Application.Interfaces.Repositories;

/// <summary>
/// Хранилище данных
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Получить событие по id
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="token">токен отмены</param>
    /// <returns>Событие</returns>
    Task<Event?> GetByIdAsync(Guid id, CancellationToken token = default);

    /// <summary>
    /// Добавить событие
    /// </summary>
    /// <param name="ev">Событие</param>
    /// <param name="token">токен отмены</param>
    /// <returns>Сохраненное событие</returns>
    Task<Event> AddAsync(Event ev, CancellationToken token = default);

    ///// <summary>
    ///// Получить все события
    ///// </summary>
    ///// <param name="token">токен отмены</param>
    ///// <returns>Список событий</returns>
    //Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken token = default);

    /// <summary>
    /// Удалить событие по id
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="token">токен отмены</param>
    /// <returns>true - удаление прошло успешно, false - ошибка при удалении</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken token = default);

    ///// <summary>
    ///// Получить количество событий
    ///// </summary>
    ///// <param name="token">токен отмены</param>
    ///// <returns>Количество событий</returns>
    //Task<int> GetCountAsync(CancellationToken token = default);

    /// <summary>
    /// Сохранить данные
    /// </summary>
    /// <param name="token">токен отмены</param>
    Task SaveChangesAsync(CancellationToken token = default);

    /// <summary>
    /// Получить все события
    /// </summary>
    /// <param name="filter">Фильтр</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Список событий</returns>
    Task<PaginatedResultDTO> GetEventsByFilterAsync(EventFilterRequestDTO filter, CancellationToken token = default);

    ///// <summary>
    ///// Вернуть событие с мягкой блокировкой
    ///// </summary>
    ///// <param name="id">Идентификатор события</param>
    ///// <param name="token">Токен отмены</param>
    ///// <returns>Событие</returns>
    ///// <exception cref="InvalidOperationException"></exception>
    //Task<Event?> GetEventWithBlockingAsync(Guid id, CancellationToken token = default);
}
