
namespace Bookings.Infrastructure.Settings.ConsumersSettings;

/// <summary>
/// Настройки для консьюмер
/// </summary>
public class EventSeatsReservedConsumerSettings
{
    /// <summary>
    /// Адрес сервера
    /// </summary>
    public string BootstrapServers { get; set; } = string.Empty;

    /// <summary>
    /// идентификатор группы консьюмеров
    /// </summary>
    public string GroupId { get; set; } = string.Empty;

    /// <summary>
    /// Название топика
    /// </summary>
    public string Topic { get; set; } = string.Empty;
}
