namespace Bookings.Application.Models.Messages;

/// <summary>
/// Тип для хранения событий в таблице Inbox
/// </summary>
/// <param name="MessageId"></param>
public record InboxMessage
(
    Guid MessageId
);