using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Models;
namespace EventManagement.Services;

/// <summary>
/// Класс валидации событий
/// </summary>
public class EventValidator : IEventValidator
{
    /// <summary>
    /// Проверить событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <exception cref="EventValidationException">Возникает, если событие не прошло проверку</exception>
    public void  Validate(EventRequestDto @event)
    {
        if (@event.EndAt < @event.StartAt)
            throw new EventValidationException("Событие содержит некорректные данные. Дата окончания меньше даты начала.");
    }
}
