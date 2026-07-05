namespace Bookings.Application.Interfaces.Consumers;

/// <summary>
/// Коньюмер для приёма сообщений об удачнос списание мест в событии
/// </summary>
public interface IEventSeatsReservedConsumer
{
    /// <summary>
    /// Получить сообщение
    /// </summary>
    /// <param name="token">Токен отмены</param>
    Task ConsumeAsync(CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    void Close();
}
