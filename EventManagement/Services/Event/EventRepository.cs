using EventManagement.Common;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using System.Collections.Concurrent;

namespace EventManagement.Services;

/// <summary>
/// Хранилище данных
/// </summary>
public class EventRepository : IEventRepository
{
    private readonly ConcurrentDictionary<Guid, Event> _repository = new (TestData.GetTestData());
        
    /// <summary>
    /// Коллекция событий
    /// </summary>
    public ConcurrentDictionary<Guid, Event> Data => _repository;


    /// <summary>
    /// Получить событие по id
    /// </summary>
    /// <param name="id">id события</param>
    /// <returns>Событие</returns>
    public Event GetByID(Guid id)
    {
        if (!_repository.TryGetValue(id, out var ev))
            throw new ArgumentException("Ошибка при получении события по id");

        return ev.Clone();
    }

    /// <summary>
    /// Добавить событие
    /// </summary>
    /// <param name="ev">Событие</param>
    public void Add(Event ev)
    {
        if (!_repository.TryAdd(ev.Id, ev))
            throw new InvalidOperationException("Ошибка при добавлении нового события");
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="ev">Событие</param>
    public void Update(Event ev)
    {
        if (!_repository.TryGetValue(ev.Id, out var oldEvent))
            throw new ArgumentException("Ошибка при получении события по id");

        if (!_repository.TryUpdate(ev.Id, ev, oldEvent))
            throw new InvalidOperationException("Ошибка при обновлении события");
    }

    /// <summary>
    /// Получить все события
    /// </summary>
    /// <returns>Список событий</returns>
    public IEnumerable<Event> GetAll()
    {
        return _repository.Select(o => o.Value.Clone());
    }

    /// <summary>
    /// Удалить событие по id
    /// </summary>
    /// <param name="id">id события</param>
    public void Delete(Guid id)
    {
        if (!_repository.TryRemove(id, out var ev))
            throw new ArgumentException("Ошибка при удалениисобытия");
    }

    /// <summary>
    /// Получить количество событий
    /// </summary>
    /// <returns>Количество событий</returns>
    public int GetCount()
    {
        return _repository.Count;
    }
}
