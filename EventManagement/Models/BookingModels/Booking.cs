using System.Diagnostics.CodeAnalysis;

namespace EventManagement.Models.BookingModels;

/// <summary>
/// Класс бронирования
/// </summary>
public class Booking
{
    private Guid _id = Guid.NewGuid();
    private BookingStatus _status;
    private Guid _eventId;
    private int _seatsCount;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="status">Статус брони</param>    
    /// <param name="eventId">id  события</param>
    /// <param name="seatsCount">количество мест в брони</param>
    /// <param name="createdAt">Дата создания брони</param>
    [SetsRequiredMembers]
    public Booking(BookingStatus status, Guid eventId, int seatsCount, DateTime createdAt)
    {
        _eventId = eventId;
        _status = status; 
        CreatedAt = createdAt;
        _seatsCount = seatsCount;
    }

    private Booking() { }

    /// <summary>
    /// Идентификатор брони
    /// </summary>
    public Guid Id => _id;

    /// <summary>
    /// Идентификатор события, к которому привязана бронь
    /// </summary>
    public Guid EventId => _eventId;

    /// <summary>
    /// Текущий статус брони
    /// </summary>
    public BookingStatus Status  => _status;

    /// <summary>
    /// Кр=оличество мест в брони
    /// </summary>
    public int SeatsCount => _seatsCount;

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
            _id = Id,
            _status = Status,
            _eventId = EventId,
            _seatsCount = SeatsCount,
            ProcessedAt = ProcessedAt
        };
    }

    /// <summary>
    /// Подтвердить бронирование
    /// </summary>
    public void Confirm()
    {
        _status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.Now;
    }

    /// <summary>
    /// Отклонить бронирование
    /// </summary>
    public void Reject()
    {
        _status = BookingStatus.Rejected;
        ProcessedAt = DateTime.Now;
    }
}
