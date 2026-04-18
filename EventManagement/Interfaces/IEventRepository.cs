using EventManagement.Models.Events;
using System.Collections.Concurrent;

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
    Event GetByID(Guid id);

    /// <summary>
    /// Добавить событие
    /// </summary>
    /// <param name="ev">Событие</param>
    Event Add(Event ev);

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="ev">Измененное событие</param>
    Event Update(Event ev);


    /// <summary>
    /// Получить все события
    /// </summary>
    /// <returns>Список событий</returns>
    IEnumerable<Event> GetAll();

    /// <summary>
    /// Удалить событие по id
    /// </summary>
    /// <param name="id">id события</param>
    Event Delete(Guid id);

    /// <summary>
    /// Получить количество событий
    /// </summary>
    /// <returns>Количество событий</returns>
    int GetCount();
}
