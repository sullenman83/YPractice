using System.Diagnostics.CodeAnalysis;

namespace EventManagement.Domain.Models;

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
    /// <param name="userId">id  пользователя</param>
    /// <param name="seatsCount">количество мест в брони</param>
    /// <param name="createdAt">Дата создания брони</param>
    [SetsRequiredMembers]
    public Booking(BookingStatus status, Guid eventId, Guid userId, int seatsCount, DateTimeOffset createdAt)
    {
        if (seatsCount <= 0)
            throw new ArgumentException("Количество бронируемых мест должно быть больше 0");
        Id = Guid.NewGuid();
        EventId = eventId;
        UserId = userId;
        Status = status;
        CreatedAt = createdAt;
        SeatsCount = seatsCount;
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="status">Статус брони</param>    
    /// <param name="ev">событие</param>
    /// <param name="user">пользователь</param>
    /// <param name="seatsCount">количество мест в брони</param>
    /// <param name="createdAt">Дата создания брони</param>
    [SetsRequiredMembers]
    public Booking(BookingStatus status, Event ev, User user, int seatsCount, DateTimeOffset createdAt) : this(status, ev.Id, user.Id, seatsCount, createdAt)
    {
        Event = ev;
        User = user;
    }
    private Booking() { }

    /// <summary>
    /// Идентификатор брони
    /// </summary>
    public Guid Id { get; init; }
    
    /// <summary>
    /// Текущий статус брони
    /// </summary>
    public BookingStatus Status { get; private set; }

    /// <summary>
    /// Кр=оличество мест в брони
    /// </summary>
    public required int SeatsCount { get; init; }

    /// <summary>
    /// Дата и время создания брони. Формат времени dd.MM.yyyy hh:mm:ssZ
    /// </summary>
    public required DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Дата и время обработки брони. Формат времени dd.MM.yyyy hh:mm:ssZ
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; set; }
        
    /// <summary>
    /// Идентификатор события, к которому привязана бронь
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    /// Событие
    /// </summary>
    public Event? Event { get; init; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Пользователь
    /// </summary>
    public User? User { get; set; }
    
    /// <summary>
    /// Подтвердить бронирование
    /// </summary>
    public void Confirm(DateTimeOffset dateTime)
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = dateTime;
    }

    /// <summary>
    /// Отменить бронирование
    /// </summary>
    /// <param name="dateTime">Дата</param>
    public void Cancel(DateTimeOffset dateTime)
    {
        Status = BookingStatus.Cancelled;
        ProcessedAt = dateTime;
    }
    /// <summary>
    /// Отклонить бронирование
    /// </summary>
    public void Reject(DateTimeOffset dateTime)
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = dateTime;
    }    
}
