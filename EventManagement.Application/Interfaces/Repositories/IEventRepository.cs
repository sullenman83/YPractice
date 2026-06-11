using EventManagement.Application.Models.Events;
using EventManagement.Application.Models.FilterModels;
using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Repositories;

/// <summary>
/// Хранилище данных
/// </summary>
public interface IEventRepository<T> : IBaseRepository<T>
{
    /// <summary>
    /// Получить все события
    /// </summary>
    /// <param name="filter">Фильтр</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Список событий</returns>
    Task<PaginatedResultDTO> GetEventsByFilterAsync(EventFilterRequestDTO filter, CancellationToken token = default);

    /// <summary>
    /// Вернуть событие с мягкой блокировкой
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Событие</returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<Event?> GetEventWithBlockingAsync(Guid id, CancellationToken token = default);
}
