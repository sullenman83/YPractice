namespace Contracts;

/// <summary>
/// Контракт для обмена сообщениями между сервисом бронирования и сервисом событий
/// Сообщение о создании бронирования
/// </summary>
public record BookingConfirmed
(
    Guid MessageId,

    Guid BookingId,

    Guid EventId,

    Guid UserId,

    int SeatsCount
);
