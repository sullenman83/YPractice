using EventManagement.Common;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;

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
        if (seatsCount <= 0)
            throw new ArgumentException("Количество бронируемых мест должно быть больше 0");
        Id = Guid.NewGuid();
        EventId = eventId;
        Status = status;
        CreatedAt = createdAt;
        SeatsCount = seatsCount;
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="status">Статус брони</param>    
    /// <param name="ev">событие</param>
    /// <param name="seatsCount">количество мест в брони</param>
    /// <param name="createdAt">Дата создания брони</param>
    [SetsRequiredMembers]
    public Booking(BookingStatus status, Event ev, int seatsCount, DateTimeOffset createdAt) : this(status, ev.Id, seatsCount, createdAt)
    {
        Event = ev;        
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
    /// Подтвердить бронирование
    /// </summary>
    public void Confirm(IDateTimeProvider dateTimeProvider)
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = dateTimeProvider.UtcNow;
    }

    /// <summary>
    /// Отклонить бронирование
    /// </summary>
    public void Reject(IDateTimeProvider dateTimeProvider)
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = dateTimeProvider.UtcNow;
    }    
}
