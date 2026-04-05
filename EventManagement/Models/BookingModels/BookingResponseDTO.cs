namespace EventManagement.Models.BookingModels;

/// <summary>
/// Класс со свойствами заявки заявки для возврата из апи
/// </summary>
public class BookingResponseDTO
{
    /// <summary>
    /// Идентификатор брони
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Идентификатор события, к которому привязана бронь
    /// </summary>
    public required Guid EventId { get; set; }

    /// <summary>
    /// Текущий статус брони
    /// </summary>
    public required BookingStatus Status { get; set; }
}
