namespace Bookings.Application.Models.Messages;

/// <summary>
/// Тип для храенения outbox событий
/// </summary>
public record OutboxMessage
(
    string MessageType,

    DateTimeOffset OccuredOn,

    string Payload,

    int RetryCount,

    bool Processed
);