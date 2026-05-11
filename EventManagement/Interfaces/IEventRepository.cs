using EventManagement.Models.Events;

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
    /// <returns>Событие</returns>
    Task<Event> GetEventByIDAsync(Guid id);

    /// <summary>
    /// Добавить событие
    /// </summary>
    /// <param name="ev">Событие</param>
    Task<Event> AddEventAsync(Event ev);

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="ev">Измененное событие</param>
    Task<Event> UpdateEventAsync(Event ev);

    /// <summary>
    /// Получить все события
    /// </summary>
    /// <returns>Список событий</returns>
    Task<IReadOnlyList<Event>> GetEventsAsync();

    /// <summary>
    /// Удалить событие по id
    /// </summary>
    /// <param name="id">id события</param>
    Task<Event> DeleteEventAsync(Guid id);

    /// <summary>
    /// Получить количество событий
    /// </summary>
    /// <returns>Количество событий</returns>
    Task<int> GetEventsCountAsync();
}
