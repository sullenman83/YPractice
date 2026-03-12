using EventManagement.Interfaces;
using EventManagement.Models;
namespace EventManagement.Services;

/// <summary>
/// Класс валидации событий
/// </summary>
public class EventValidator : IEventValidator
{
    public void  Validate(Event @event)
    {
        if (@event.EndAt < @event.StartAt)
            throw new ArgumentException("Событие содержит некорректные данные. Дата окончания меньше даты начала.");
    }
}
