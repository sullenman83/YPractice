namespace EventManagement.Application.Common.AppSettings;

/// <summary>
/// Настройки для репитера
/// </summary>
internal class RetrySettings
{
    /// <summary>
    /// Задержка между повторными вызовами (мс)
    /// </summary>
    public int Delay {  get; set; }

    /// <summary>
    /// Максимально число повторов
    /// </summary>
    public int MaxRetryAttempts { get; set; }    
}

/// <summary>
/// Настройки для повторителя в срвисе создания бронирований
/// </summary>
internal class CreateBookingRetrySettigs : RetrySettings 
{
    /// <summary>
    /// Максмальное время для выполнения операции
    /// </summary>
    public int Timeout { get; set; }
}

/// <summary>
/// Настройки для повторителя в фоновом срвисе обработки бронирований
/// </summary>
internal class BackgroundBookingServiceRetrySettigs : RetrySettings { }
