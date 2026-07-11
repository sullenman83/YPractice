using Contracts;

namespace Events.Application.Interfaces.Consumers;

/// <summary>
/// Интерфейс получения и обработки сообщений BookingConfirmed 
/// </summary>
public interface IBookingConfirmedConsumer: IDisposable
{
    /// <summary>
    /// Получить сообщение BookingConfirm и обработать его
    /// </summary>
    /// <param name="messageHandler">Обработчик сообщения</param>
    /// <param name="token">токен отмены</param>
    Task ConsumeAsync(Func<BookingConfirmed, CancellationToken, Task> messageHandler, CancellationToken token);
}
