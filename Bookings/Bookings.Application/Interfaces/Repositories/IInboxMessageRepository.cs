using Bookings.Application.Models.Messages;

namespace Bookings.Application.Interfaces.Repositories;

/// <summary>
/// Интерфейс хранилища входящих сообщений 
/// </summary>
public interface IInboxMessageRepository
{
    /// <summary>
    /// Добавить сообщение
    /// </summary>
    /// <param name="message">Объект сообщения</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Добавленное сообщение</returns>
    Task<InboxMessage> AddAsync(InboxMessage message, CancellationToken token = default);
}
