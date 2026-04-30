using System.Diagnostics.CodeAnalysis;

namespace EventManagement.Models.BookingModels;

/// <summary>
/// Класс бронирования
/// </summary>
public class Booking
{
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="status">Статус брони</param>    
    /// <param name="eventId">id  события</param>
    /// <param name="seatsCount">количество мест в брони</param>
    /// <param name="createdAt">Дата создания брони</param>
    [SetsRequiredMembers]
    public Booking(BookingStatus status, Guid eventId, int seatsCount, DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        Status = status;
        CreatedAt = createdAt;
        SeatsCount = seatsCount;
    }

    private Booking() { }

    /// <summary>
    /// Идентификатор брони
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Идентификатор события, к которому привязана бронь
    /// </summary>
    public Guid EventId { get; init; }

    /// <summary>
    /// Текущий статус брони
    /// </summary>
    public BookingStatus Status { get; private set; }

    /// <summary>
    /// Кр=оличество мест в брони
    /// </summary>
    public int SeatsCount { get; init; }

    /// <summary>
    /// Дата и время создания брони
    /// </summary>
    public required DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Дата и время обработки брони
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; set; }


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
            SeatsCount = SeatsCount,
            ProcessedAt = ProcessedAt
        };
    }

    /// <summary>
    /// Подтвердить бронирование
    /// </summary>
    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Отклонить бронирование
    /// </summary>
    public void Reject()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTimeOffset.UtcNow;
    }
}
