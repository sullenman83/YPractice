namespace Bookings.Application.Models;

/// <summary>
/// Тип для храенения outbox событий
/// </summary>
public record OutboxMessage
(   
    Guid MessageId,

    string MessageType,

    DateTimeOffset OccuredOn,

    string Payload,

    int RetryCount,

    bool Processed
);