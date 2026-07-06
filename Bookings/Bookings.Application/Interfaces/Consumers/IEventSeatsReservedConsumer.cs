namespace Bookings.Application.Interfaces.Consumers;

/// <summary>
/// Коньюмер для приёма сообщений об удачнос списание мест в событии
/// </summary>
public interface IEventSeatsReservedConsumer: IDisposable
{
    /// <summary>
    /// Получить сообщение
    /// </summary>
    Task Consume();
}
