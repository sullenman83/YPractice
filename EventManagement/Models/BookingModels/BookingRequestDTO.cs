using System.ComponentModel.DataAnnotations;

namespace EventManagement.Models.BookingModels;

/// <summary>
/// Класс запроса на создание брони
/// </summary>
public class BookingRequestDTO
{
    /// <summary>
    /// Идентификатор события, к которому привязана бронь
    /// </summary>
    [Required]
    public required Guid EventId { get; set; }
}
