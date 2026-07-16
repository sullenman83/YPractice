namespace Events.Application.Settings;

/// <summary>
/// Настройки TTL различных объектов
/// </summary>
public class TTLSettings
{
    /// <summary>
    /// TTL дл событий
    /// </summary>
    public int EventTTL { get; set; }

    /// <summary>
    /// TTL для списка топ10 событий
    /// </summary>
    public int Top10TTL { get; set; }
}
