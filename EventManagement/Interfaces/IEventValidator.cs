using EventManagement.Models;

namespace EventManagement.Interfaces;

public interface IEventValidator
{
    /// <summary>
    /// Проверить событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <exception cref="ArgumentException">Возникает если дата окончения меньше даты начала</exception>
    void Validate(Event @event);
}
