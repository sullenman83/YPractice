namespace EventManagement.Common.AppSettings;

/// <summary>
/// Настройки для фонового сервиса обработки бронирований
/// </summary>
public class BookingHandlerSettings
{
    /// <summary>
    /// Максимальная продолжительность обработки бронирования (мс)
    /// </summary>
    public int MaxProccessingDuration { get; set; }

    /// <summary>
    /// Продолжительность эмитации обращения к внешнему сервису 
    /// </summary>
    public int ProcessingDelay { get; set; }

    /// <summary>
    /// Провежуток между запуском обработки бронирований
    /// </summary>
    public int PollingInterval { get; set; }
}
