namespace Events.Application.Common;

/// <summary>
/// Ключи для доступа к кешированным данным
/// </summary>
public static class CacheKeys
{
    private const string EventPrefix = "event";
    
    /// <summary>
    /// Ключ для события
    /// </summary>
    /// <param name="eventId">идентификатор события</param>
    /// <returns>ключ</returns>
    public static string EventKey(Guid eventId) => $"{EventPrefix}:{eventId}";

    /// <summary>
    /// Ключ для топ 10 событий
    /// </summary>
    /// <returns>Ключ</returns>
    public static string Top10EventsKey() => "events:top10";
}
