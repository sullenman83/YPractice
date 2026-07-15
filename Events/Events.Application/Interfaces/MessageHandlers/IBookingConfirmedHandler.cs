using Contracts;

namespace Events.Application.Interfaces.MessageHandlers;

/// <summary>
/// Интерфейс обработки сообщения BookingConfirmed
/// </summary>
public interface IBookingConfirmedHandler
{
    /// <summary>
    /// Обработать сообщение
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="token">токен отмены</param>    
    Task HandleMessageAsync(BookingConfirmed message, CancellationToken token);
}
