using EventManagement.Interfaces;

namespace EventManagement.Services;

/// <inheritdoc/>>
public class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc/>>
    public DateTimeOffset UtcNow
    {
        get 
        { 
            var d = DateTimeOffset.UtcNow;
            return new DateTimeOffset(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, 0, d.Offset);    
        }
    }
}
