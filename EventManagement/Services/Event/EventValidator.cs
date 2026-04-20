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
    /// <param name="ev">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <exception cref="EventValidationException">Возникает, если событие не прошло проверку</exception>
    public async Task ValidateAsync(EventCreationDTO ev, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        ValidateDate(ev.StartAt, ev.EndAt);
    }

    /// <summary>
    /// Проверить событие
    /// </summary>
    /// <param name="ev">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <exception cref="EventValidationException">Возникает, если событие не прошло проверку</exception>
    public async Task ValidateAsync(EventUpdateDTO ev, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        ValidateDate(ev.StartAt, ev.EndAt);
    }

    private void ValidateDate(DateTimeOffset? statrtAt, DateTimeOffset? endAt)
    {
        if (endAt < statrtAt)
            throw new EventValidationException("Событие содержит некорректные данные. Дата окончания меньше даты начала.");
    }
}
