namespace Contracts;

public record EventSeatsReserved
(
    Guid MessageId,

    Guid BookingId,

    Guid EventId,

    Guid UserId
);
