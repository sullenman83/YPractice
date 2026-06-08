namespace EventManagement.Application.Common.AppSettings;

/// <summary>
/// Настройки для фонового сервиса обработки бронирований
/// </summary>
public class BookingHandlerSettings
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
