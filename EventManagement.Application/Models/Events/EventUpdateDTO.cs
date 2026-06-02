using System.ComponentModel.DataAnnotations;

namespace EventManagement.Models.Events;

/// <summary>
/// Клас для обновления события
/// </summary>
public class EventUpdateDTO
{
    /// <summary>
    /// Название события
    /// </summary>
    [Required]
    public required string Title { get; set; }

    /// <summary>
    /// Описание события
    /// </summary>    
    public string? Description { get; set; }

    /// <summary>
    /// Дата и время начала события. Формат времени dd.MM.yyyy hh:mm:ssZ
    /// </summary>
    /// <example>2026-05-15T12:03:24Z</example>
    [Required]
    public required DateTimeOffset? StartAt { get; set; }

    /// <summary>
    /// Дата и время окончания события. Формат времени dd.MM.yyyy hh:mm:ssZ
    /// </summary>
    /// <example>2026-05-15T12:03:24Z</example>
    [Required]
    public required DateTimeOffset? EndAt { get; set; }
}
