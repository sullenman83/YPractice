using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;

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
    Task<Event?> GetEventByIDAsync(Guid id, CancellationToken token);

    /// <summary>
    /// Добавить событие
    /// </summary>
    /// <param name="ev">Событие</param>
    /// <param name="token">токун отмены</param>
    /// <returns>Сохраненное событие</returns>
    Task<Event> AddEventAsync(Event ev, CancellationToken token);
        

    /// <summary>
    /// Получить все события
    /// </summary>
    /// <returns>Список событий</returns>
    Task<IReadOnlyList<Event>> GetEventsAsync(EventFilterRequestDTO filter, CancellationToken token);

    /// <summary>
    /// Удалить событие по id
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="token">токун отмены</param>
    /// <returns>true - удаление прошло успешно, false - ошибка при удалении</returns>
    Task<bool> DeleteEventAsync(Guid id, CancellationToken token);

    /// <summary>
    /// Получить количество событий
    /// </summary>
    /// <param name="token">токун отмены</param>
    /// <returns>Количество событий</returns>
    Task<int> GetEventsCountAsync(CancellationToken token);

    /// <summary>
    /// Сохранить данные
    /// </summary>
    /// <param name="token">токун отмены</param>
    Task SaveChangesAsync(CancellationToken token);
}
