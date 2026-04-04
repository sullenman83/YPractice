namespace EventManagement.Models.BookingModels;

/// <summary>
/// Класс бронирования
/// </summary>
public class Booking
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
    public required BookingStatus Status  { get; set; }

    /// <summary>
    /// Дата и время создания брони
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата и время обработки брони
    /// </summary>
    public DateTime? ProcessedAt { get; set; }


    /// <summary>
    /// Создать клон объекта
    /// </summary>
    /// <returns></returns>
    public Booking Clone()
    {
        return new Booking()
        {
            CreatedAt = CreatedAt,
            Id = Id,
            Status = Status,
            EventId = EventId,
            ProcessedAt = ProcessedAt
        };
    }
}
