
namespace Bookings.Application.AppSettings;

/// <summary>
/// Настройки для исходящих сообщений (типы)
/// </summary>
public class OutboxMessageSettings
{
    /// <summary>
    /// Название типа для исходящих сообщений создания брони
    /// </summary>
    public string CreateBooking { get; set; } = string.Empty;

    /// <summary>
    /// Название типа для исходящих сообщений отмены брони
    /// </summary>
    public string CancelBooking { get; set; } = string.Empty;
}
