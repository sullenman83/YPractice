using EventManagement.Data;
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
    /// <param name="filter">Фильтр</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Список событий</returns>
    Task<PaginatedResultDTO> GetEventsByFilterAsync(EventFilterRequestDTO filter, CancellationToken token = default);
}
