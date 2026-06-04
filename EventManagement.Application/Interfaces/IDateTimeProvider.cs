namespace EventManagement.Application.Interfaces;

/// <summary>
/// Генератор времени
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Вернуть текущее utc время 
    /// </summary>
    public DateTimeOffset GetUtcNow();
}
