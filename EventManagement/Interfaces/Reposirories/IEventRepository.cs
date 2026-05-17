using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace EventManagement.Interfaces.Reposirories;

/// <summary>
/// Хранилище данных
/// </summary>
public interface IEventRepository<T> : IBaseRepository<T>
{
    /// <summary>
    /// Получить все события
    /// </summary>
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
