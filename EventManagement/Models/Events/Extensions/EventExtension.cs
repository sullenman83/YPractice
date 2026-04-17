using EventManagement.Models.Events;

namespace EventManagement.Models.Events.Extensions;

/// <summary>
/// расширения
/// </summary>
public static class EventExtension
{
    /// <summary>
    /// Сконвертировать Event в EventResponseDto
    /// </summary>
    /// <param name="ev">Событие</param>
    /// <returns>Ответ</returns>
    public static EventResponseDto ToResponse(this Event ev)
    {
        return new EventResponseDto()
        {
            Id = ev.Id,
            Title = ev.Title,
            Description = ev.Description,
            StartAt = ev.StartAt,
            EndAt = ev.EndAt,
            TotalSeats = ev.TotalSeats,
            AvailableSeats = ev.AvailableSeats
        };
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="ev">Событие</param>
    /// <param name="source">Данные для обновления</param>
    /// <returns>Обновленное событие</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Event Update(this Event ev, UpdateEventDTO source)
    {
        ev.EndAt = source.EndAt ?? throw new ArgumentNullException("EndAt не может быть null");
        ev.StartAt = source.StartAt ?? throw new ArgumentNullException("StartAt не может быть null");
        ev.Title = source.Title;
        ev.Description = source.Description;

        return ev;
    }
}
