
using Contracts;

namespace Events.Application.Interfaces.Consumers;

/// <summary>
/// Интерфейс получения и обработки сообщений BookingCabcelled 
/// </summary>
public interface IBookingCancelledConsumer: IDisposable
{
    /// <summary>
    /// Получить сообщение BookingCancelled и обработать его
    /// </summary>
    /// <param name="messageHandler">Обработчик сообщения</param>
    /// <param name="token">токен отмены</param>
    void Consume(Func<BookingCancelled, CancellationToken, Task> messageHandler, CancellationToken token);
}
