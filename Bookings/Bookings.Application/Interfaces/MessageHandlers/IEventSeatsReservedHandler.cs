

using Contracts;

namespace Bookings.Application.Interfaces.MessageHandlers;

/// <summary>
/// Обработчик сообщений о удачом списании мест события
/// </summary>
public interface IEventSeatsReservedHandler
{
    /// <summary>
    /// Обработать сообщение 
    /// </summary>
    /// <param name="message">Сообщение</param>
    Task HandleMessage(EventSeatsReserved message);
}
