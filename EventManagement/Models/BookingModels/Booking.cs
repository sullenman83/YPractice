namespace EventManagement.Models.BookingModels;

/// <summary>
/// Класс бронирования
/// </summary>
public class Booking
{
    private Guid _id = Guid.NewGuid();
    private BookingStatus _status;
    private Guid _eventId;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="status">Статус брони</param>
    public Booking(BookingStatus status, Guid eventID)
    {
        _eventId = eventID;
        _status = status; 
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
            ProcessedAt = ProcessedAt
        };
    }

    /// <summary>
    /// Подтвердить бронирование
    /// </summary>
    public void Confirm()
    {
        _status = BookingStatus.Confirmed;
    }

    /// <summary>
    /// Отклонить бронирование
    /// </summary>
    public void Reject()
    {
        _status = BookingStatus.Rejected;
    }
}
