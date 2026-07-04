
namespace Bookings.Application.Models;

/// <summary>
/// Тип для хранения событий в таблице Inbox
/// </summary>
/// <param name="MessageId"></param>
public record InboxMessage
(
    Guid MessageId
);