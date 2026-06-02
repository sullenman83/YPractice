namespace EventManagement.Interfaces.Services;

/// <summary>
/// Интерфейс обработки броней
/// </summary>
public interface IBackgroundBookingService
{
    /// <summary>
    /// Подтвердить бронирование
    /// </summary>
    /// <param name="id">Идентификатор бронирования</param>
    /// <param name="token">Токен отмены</param>
    Task ConfirmBookingAsync(Guid id, CancellationToken token);

    /// <summary>
    /// Отменить бронирование
    /// </summary>
    /// <param name="id">Идентификатор бронирования</param>
    /// <param name="token">Токен отмены</param>
    Task RejectBookingAsync(Guid id, CancellationToken token);
}
