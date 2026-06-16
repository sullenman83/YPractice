using EventManagement.Application.Interfaces.Services.EventServices;
using EventManagement.Application.Models.Events;
using EventManagement.Domain.Exceptions;

namespace EventManagement.Application.Services.EventServices;

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

        if (startAt.HasValue && (startAt.Value.Microsecond != 0 || startAt.Value.Millisecond != 0))
            throw new EventValidationException("Неверный формат даты начала события. Значение микросекунд и миллисекунд должны быть 0");

        if (endAt.HasValue && (endAt.Value.Microsecond != 0 || endAt.Value.Millisecond != 0))
            throw new EventValidationException("Неверный формат даты окончания события. Значение микросекунд и миллисекунд должны быть 0");
    }
}
