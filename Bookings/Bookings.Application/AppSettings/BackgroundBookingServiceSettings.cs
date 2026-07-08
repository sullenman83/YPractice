namespace Bookings.Application.AppSettings;

/// <summary>
/// Настройки для фонового сервиса обработки бронирований
/// </summary>
public class BackgroundBookingServiceSettings
{
    /// <summary>
    /// Продолжительность эмитации обращения к внешнему сервису 
    /// </summary>
    public int ProcessingDelay { get; set; }

    /// <summary>
    /// Провежуток между запуском обработки бронирований
    /// </summary>
    public int PollingInterval { get; set; }
}
