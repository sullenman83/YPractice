using EventManagement.Application.Common.Exceptions;
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
    /// <exception cref="EventValidationException">Возникает, если событие не прошло проверку</exception>
    void Validate(EventCreationDTO @event);

    /// <summary>
    /// Проверить событие
    /// </summary>
    /// <param name="event">Данные события</param>    
    /// <exception cref="EventValidationException">Возникает, если событие не прошло проверку</exception>
    void Validate(EventUpdateDTO @event);
}
