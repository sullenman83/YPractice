namespace EventManagement.Interfaces;

/// <summary>
/// Генератор времени
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Текущее utc время 
    /// </summary>
    public DateTimeOffset UtcNow { get; }
}
