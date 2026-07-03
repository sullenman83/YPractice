namespace Contracts;

public record EventSeatsNotReserved
(
    Guid MessageId,

    Guid BookingId,

    Guid EventId,

    Guid UserId,

    string Reason
);
