namespace EventManagement.Models.Events.Extensions;

/// <summary>
/// Расширения
/// </summary>
public static class CreateEventDTOExtension
{

    /// <summary>
    /// Сконвертировать в Event
    /// </summary>
    /// <param name="source">Данные для Event</param>
    /// <returns>Объект события</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Event ToEvent(this CreateEventDTO source)
    {
        return new Event(
            source.Title,
            source.Description,
            source.StartAt ?? throw new ArgumentNullException("StartAt не может быть null"),
            source.EndAt ?? throw new ArgumentNullException("EndAt не может быть null"),
            source.TotalSeats ?? throw new ArgumentNullException("TotalSeats не может быть null")
        );
    }
}
