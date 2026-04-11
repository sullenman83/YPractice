using EventManagement.Common.Exceptions;
using EventManagement.Models.Events;

namespace EventManagement.Interfaces;

/// <summary>
/// Интерфейс валидатора событий
/// </summary>
public interface IEventValidator
{
    /// <summary>
    /// Проверить событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <exception cref="EventValidationException">Возникает, если событие не прошло проверку</exception>
    Task Validate(EventRequestDto @event, CancellationToken token);
}
