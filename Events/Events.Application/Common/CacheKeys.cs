namespace Events.Application.Common;

/// <summary>
/// Ключи для доступа к кешированным данным
/// </summary>
public static class CacheKeys
{
    private const string EventPrefix = "event";
    private const string Top10 = "top10";
    
    /// <summary>
    /// Ключ для события
    /// </summary>
    /// <param name="eventId">идентификатор события</param>
    /// <returns>ключ</returns>
    public static string EventKey(Guid eventId) => $"{EventPrefix}:{eventId}";
}
