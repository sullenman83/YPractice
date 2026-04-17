using System.ComponentModel.DataAnnotations;

namespace EventManagement.Models.Events;

/// <summary>
/// Клас для обновления события
/// </summary>
public class UpdateEventDTO
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
    /// Дата и время начала события
    /// </summary>
    [Required]
    public required DateTime? StartAt { get; set; }

    /// <summary>
    /// Дата и время окончания события
    /// </summary>
    [Required]
    public required DateTime? EndAt { get; set; }
}
