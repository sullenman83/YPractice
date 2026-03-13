using System.ComponentModel.DataAnnotations;

namespace EventManagement.Models;


/// <summary>
/// DTO класс для передачи данных события из Web API
/// </summary>
public class EventResponseDto
{
    /// <summary>
    /// Идентификатор события
    /// </summary>    
    public required int Id { get; set; }

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
