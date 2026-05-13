using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace EventManagement.Interfaces;

/// <summary>
/// Хранилище данных
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Получить событие по id
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="token">токун отмены</param>
    /// <returns>Событие</returns>
    Task<Event?> GetEventByIdAsync(Guid id, CancellationToken token = default);

    /// <summary>
    /// Добавить событие
    /// </summary>
    /// <param name="ev">Событие</param>
    /// <param name="token">токун отмены</param>
    /// <returns>Сохраненное событие</returns>
    Task<Event> AddEventAsync(Event ev, CancellationToken token = default);
        

    /// <summary>
    /// Получить все события
    /// </summary>
    /// <returns>Список событий</returns>
    Task<IReadOnlyList<Event>> GetEventsAsync(EventFilterRequestDTO filter, CancellationToken token = default);

    /// <summary>
    /// Удалить событие по id
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="token">токун отмены</param>
    /// <returns>true - удаление прошло успешно, false - ошибка при удалении</returns>
    Task<bool> DeleteEventAsync(Guid id, CancellationToken token = default);

    /// <summary>
    /// Получить количество событий
    /// </summary>
    /// <param name="token">токун отмены</param>
    /// <returns>Количество событий</returns>
    Task<int> GetEventsCountAsync(CancellationToken token = default);

    /// <summary>
    /// Сохранить данные
    /// </summary>
    /// <param name="token">токун отмены</param>
    Task SaveChangesAsync(CancellationToken token = default);


    /// <summary>
    /// Создать транзакцию
    /// </summary>
    /// <param name="token">Токен оотмены</param>
    /// <returns>Транзакция</returns>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken token = default);

    /// <summary>
    /// Вернуть событие с мягкой блокировкой
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Событие</returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<Event?> GetEventWithBlockingAsync(Guid id, CancellationToken token = default);
}
