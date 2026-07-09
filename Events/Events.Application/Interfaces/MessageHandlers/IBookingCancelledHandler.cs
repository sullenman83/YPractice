using Contracts;

namespace Events.Application.Interfaces.MessageHandlers;

/// <summary>
/// Интерфейс обработчика сообщения BookingCancelled
/// </summary>
internal interface IBookingCancelledHandler
{
    /// <summary>
    /// Обработать сообщение
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="token">токен отмены</param>    
    Task HandleMessage(BookingCancelled message, CancellationToken token);
}
