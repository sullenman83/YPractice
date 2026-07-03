using DateTimeManager.Abstractions;

namespace DateTimeManager.Core;

public class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc/>>
    /// Возвращает врем с точностью до 6 знаков в микросекундах
    public DateTimeOffset GetUtcNow()
    {
        var d = DateTimeOffset.UtcNow;
        return new DateTimeOffset(d.Ticks - d.Ticks % 10, d.Offset);
    }
}
