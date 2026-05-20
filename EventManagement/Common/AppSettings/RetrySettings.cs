namespace EventManagement.Common.AppSettings;

/// <summary>
/// Натсройки для репитера
/// </summary>
public class RetrySettings
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
