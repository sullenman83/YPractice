namespace Bookings.Application.AppSettings;

/// <summary>
/// Настройки фонового сервиса публикации outbox сообщений в кафка
/// </summary>
public class BackgroundProducerServiceSettings
{
    /// <summary>
    /// Интервал между отправками (в миллисекундах)
    /// </summary>
    public int PollingInterval { get; set; } = 5000;

    /// <summary>
    /// Сопоставление тип события - название топика
    /// </summary>
    public Dictionary<string, string> Topics { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Максимальное количество повторных отправлений
    /// </summary>
    public int MaxRetryCount = 3;
}
