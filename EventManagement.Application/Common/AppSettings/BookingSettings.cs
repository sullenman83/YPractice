namespace EventManagement.Application.Common.AppSettings;

/// <summary>
/// Настройки бронирования
/// </summary>
public class BookingSettings
{
    /// <summary>
    /// Максимальное количество активных бронирований
    /// </summary>
    public int MaxActiveBookingCount { get; set; }
}
