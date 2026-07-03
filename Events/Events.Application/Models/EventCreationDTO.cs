using System.ComponentModel.DataAnnotations;

namespace Events.Application.Models;

/// <summary>
/// DTO класс для передачи данных события в WEB API
/// </summary>
public class EventCreationDTO
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
    /// Дата и время начала события. 
    /// </summary>
    /// <example>2026-05-15T12:03:24Z</example>
    [Required]
    public required DateTimeOffset? StartAt { get; set; }

    /// <summary>
    /// Дата и время окончания события.
    /// </summary>
    /// <example>2026-05-16T12:03:24Z</example>
    [Required]
    public required DateTimeOffset? EndAt { get; set; }

    /// <summary>
    /// ОБщее количество мест
    /// </summary>
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Значени должно быть больше 0")]
    public required int? TotalSeats { get; set; }    
}
