
namespace Events.Infrastructure.Settings;

/// <summary>
/// Настройки для Redis
/// </summary>
public class RedisSettings
{
    /// <summary>
    /// Адрес или спсок адресов серверов Redis
    /// </summary>
    public string EndPoints { get; set; } = string.Empty;

    /// <summary>
    /// Таймаут при соединении с Redis
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Падать или нет при неудачном подключении
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;
}
