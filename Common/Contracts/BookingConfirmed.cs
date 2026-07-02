namespace Contracts;

/// <summary>
/// Контракт для обмена сообщениями между сервисом бронирования и сервисом событий
/// Сообщение о создании бронирования
/// </summary>
public record BookingConfirmed
{
    public Guid BookingId { get; set; }

    public Guid EventId { get; set; }

    public DateTimeOffset OccuredOn { get; set; }

    public bool IdProcessed {  get; set; }
}
