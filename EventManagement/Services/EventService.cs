using EventManagement.Interfaces;
using EventManagement.Models;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using System.Runtime.CompilerServices;

namespace EventManagement.Services;

/// <summary>
/// Сервис для работы с событиями
/// </summary>
public class EventService(IEventValidator eventValidator) : IEventService
{
    private static List<Event> _events = new List<Event>() 
    { 
        new Event { Id = 1,
            Title = "Событие 1",
            Description = "Описание 1",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1),
        },
        new Event { Id = 2,
            Title = "Событие 2",
            Description = "Описание 21",
            StartAt = DateTime.Now.AddHours(1),
            EndAt = DateTime.Now.AddDays(2),
        }
    };
    private readonly IEventValidator _eventValidator = eventValidator;

    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="event">Данные события</param>
    public void CreateEvent(EventRequestDto @event)
    {
        _eventValidator.Validate(@event);
        var ev = createEvent(@event);        
        _events.Add(ev);
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">Идентификатор удаляемого события</param>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    public void DeleteEvent(int id)
    {
        var ev = getEventById(id);

        _events.Remove(ev);
    }

    /// <summary>
    /// Получить все события
    /// </summary>
    /// <returns>Список событий</returns>
    public List<EventResponseDto> GetAllEvents()
    {
        return _events.Select(o => createEventResponseDto(o)).ToList();
    }

    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <returns>Событие с искомым идентификатором</returns>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    public EventResponseDto GetEventById(int id)
    {
        var ev = getEventById(id);

        return createEventResponseDto(ev);
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="event">Данные события</param>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    public void UpdateEvent(int id, EventRequestDto @event)
    {
        _eventValidator.Validate(@event);
        var ev = getEventById(id);
        updateEvent(@event, ev);
    }

    private Event createEvent(EventRequestDto source)
    {
        var id = _events.Count > 0 ? _events.Max(o => o.Id) + 1: 1;
        return new Event()
        {
            Id = id,
            Title = source.Title,
            Description = source.Description,
            StartAt = source.StartAt,
            EndAt = source.EndAt,
        };
    }

    private EventResponseDto createEventResponseDto(Event source)
    {
        return new EventResponseDto()
        {
            Id = source.Id,
            Title = source.Title,
            Description = source.Description,
            StartAt = source.StartAt,
            EndAt = source.EndAt,
        };
    }

    private void updateEvent(EventRequestDto source, Event dest)
    {
        dest.EndAt = source.EndAt;
        dest.StartAt = source.StartAt;
        dest.Title = source.Title;
        dest.Description = source.Description;        
    }

    private Event getEventById(int id)
    {
        var ev = _events.FirstOrDefault(o => o.Id == id);
        if (ev == null)
            throw new ArgumentException($"Не найдено событие с id = {id}");

        return ev;
    }
}
