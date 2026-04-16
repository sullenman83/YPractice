using System.ComponentModel.DataAnnotations;

namespace EventManagement.Models.Events;

/// <summary>
/// DTO класс для передачи данных события в WEB API
/// </summary>
public class EventRequestDto
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

    /// <summary>
    /// ОБщее количество мест
    /// </summary>
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Значени должно быть больше 0")]
    public required int? TotalSeats { get; set; }    
}
