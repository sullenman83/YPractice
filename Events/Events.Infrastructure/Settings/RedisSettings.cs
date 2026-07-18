
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
    /// Таймаут при соединении с Redis в мс
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Таймаут при выполнении синхронных операция с Redis в мс
    /// </summary>
    public int SyncTimeout { get; set; } = 2000;

    /// <summary>
    /// Таймаут при выполнении асинхронных операция с Redis в мс
    /// </summary>
    public int AsyncTimeout { get; set; } = 2000;

    /// <summary>
    /// Падать или нет при неудачном подключении
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;
}
