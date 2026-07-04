using Bookings.Application.Models.Messages;
using Bookings.Domain.Models;
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

    /// <summary>
    /// Получить неотправленные события
    /// </summary>
    /// <param name="token">токен отмены</param>
    /// <returns>Список сообщений с processed = false</returns>
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessed(CancellationToken token = default);

    /// <summary>
    /// Сохранить данные
    /// </summary>
    /// <param name="token">токен отмены</param>
    Task SaveChangesAsync(CancellationToken token = default);
}
