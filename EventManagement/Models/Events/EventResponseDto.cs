using System.ComponentModel.DataAnnotations;

namespace EventManagement.Models.Events;


/// <summary>
/// DTO класс для передачи данных события из Web API
/// </summary>
public class EventResponseDto
{
    /// <summary>
    /// Идентификатор события
    /// </summary>    
    public Guid Id { get; set; }

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
    public required DateTimeOffset StartAt { get; set; }

    /// <summary>
    /// Дата и время окончания события
    /// </summary>
    public required DateTimeOffset EndAt { get; set; }

    /// <summary>
    /// ОБщее количество мест
    /// </summary>
    public required int TotalSeats { get; set; }

    /// <summary>
    /// Текущее количество свободных мест
    /// </summary>
    public required int AvailableSeats { get; set; }
}
