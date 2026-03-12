namespace EventManagement.Models;

public class Event
{
    /// <summary>
    /// Идентификатор события
    /// </summary>
    public required int Id { get; set; } = -1;

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
}
