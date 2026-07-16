using Events.Application.Interfaces.Validators;
using Events.Application.Models;
using Events.Domain.Exceptions;

namespace Events.Application.Services.Validators;

/// <summary>
/// Класс валидации событий
/// </summary>
public class EventValidator : IEventValidator
{
    /// <summary>
    /// Проверить событие
    /// </summary>
    /// <param name="ev">Данные события</param>    
    /// <exception cref="EventValidationException">Возникает, если событие не прошло проверку</exception>
    public void Validate(EventCreationDTO ev)
    {
        ValidateDate(ev.StartAt, ev.EndAt);
    }

    /// <summary>
    /// Проверить событие
    /// </summary>
    /// <param name="ev">Данные события</param>
    /// <exception cref="EventValidationException">Возникает, если событие не прошло проверку</exception>
    public void Validate(EventUpdateDTO ev)
    {
        ValidateDate(ev.StartAt, ev.EndAt);
    }

    private void ValidateDate(DateTimeOffset? startAt, DateTimeOffset? endAt)
    {
        if (endAt < startAt)
            throw new EventValidationException("Событие содержит некорректные данные. Дата окончания меньше даты начала.");       
    }
}
