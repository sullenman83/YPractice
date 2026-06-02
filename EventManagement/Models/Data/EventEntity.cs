namespace EventManagement.Models.Data;

/// <summary>
/// события (сущность БД)
/// </summary>
public class EventEntity
{
    /// <summary>
    /// идентификатор события
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Название события
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание события
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата и время начала события
    /// </summary>
    public DateTimeOffset StartAt { get; set; }

    /// <summary>
    /// Дата и время окончания события
    /// </summary>
    public DateTimeOffset EndAt { get; set; }

    /// <summary>
    /// ОБщее количество мест
    /// </summary>
    public int TotalSeats { get; set; }

    /// <summary>
    /// Текущее количество свободных мест
    /// </summary>
    public int AvailableSeats { get; set; }
}
