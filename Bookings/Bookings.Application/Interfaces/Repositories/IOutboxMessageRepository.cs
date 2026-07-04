using Bookings.Application.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookings.Application.Interfaces.Repositories;

/// <summary>
/// Интерфейс хранилища исходящих сообщений 
/// </summary>
public interface IOutboxMessageRepository
{
    /// <summary>
    /// Добавить сообщение
    /// </summary>
    /// <param name="message">Объект сообщения</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Добавленное сообщение</returns>
    Task<OutboxMessage> AddAsync(OutboxMessage message, CancellationToken token = default);
}
