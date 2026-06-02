using EventManagement.Domain.Interfaces;

namespace EventManagement.Domain.Services;

/// <inheritdoc/>>
public class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc/>>
    /// Возвращает врем с точностью до секунд
    public DateTimeOffset GetUtcNow()
    {
        var d = DateTimeOffset.UtcNow;
        return new DateTimeOffset(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, 0, d.Offset);
    }
}
