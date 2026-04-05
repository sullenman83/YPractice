namespace EventManagement.Models.Events;

/// <summary>
/// Класс события
/// </summary>
public class Event
{
    /// <summary>
    /// Идентификатор события
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Название события
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Описание события
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата и время начала события
    /// </summary>
    public required DateTime StartAt { get; set; }

    /// <summary>
    /// Дата и время окончания события
    /// </summary>
    public required DateTime EndAt { get; set; }

    /// <summary>
    /// Создать корпию события
    /// </summary>
    /// <returns>Копия события</returns>
    public Event Clone()
    {
        return new Event()
        {
            Id = Id,
            Title = Title,
            Description = Description,
            StartAt = StartAt,
            EndAt = EndAt
        };
    }
}
