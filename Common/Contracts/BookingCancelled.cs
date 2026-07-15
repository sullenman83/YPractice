
namespace Contracts;

/// <summary>
/// Контракт для обмена сообщениями об отмене бронирования
/// </summary>
public record BookingCancelled
(
    Guid MessageId,

    Guid BookingId,

    Guid EventId,

    Guid UserId,

    int SeatsCount,

    DateTimeOffset CancelledDate
);
