using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
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
    /// <param name="token">Токен отмены операции</param>
    /// <exception cref="EventValidationException">Возникает, если событие не прошло проверку</exception>
    public async Task ValidateAsync(EventRequestDto @event, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (@event.EndAt < @event.StartAt)
            throw new EventValidationException("Событие содержит некорректные данные. Дата окончания меньше даты начала.");
    }
}
