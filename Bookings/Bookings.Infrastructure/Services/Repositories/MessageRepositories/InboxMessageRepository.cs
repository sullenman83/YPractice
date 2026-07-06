using Bookings.Application.Exceptions;
using Bookings.Application.Interfaces.Repositories;
using Bookings.Application.Models.Messages;
using Bookings.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookings.Infrastructure.Services.Repositories.MessageRepositories;

/// <summary>
/// Репозиторий входящих сообщений
/// </summary>
/// <param name="context">Контекст базы данных</param>
/// <param name="logger">Логгер</param>
public class InboxMessageRepository(AppDbContext context, ILogger<OutboxMessageRepository> logger) : IInboxMessageRepository
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<OutboxMessageRepository> _logger = logger;

    ///<inheritdoc/>
    public async Task<InboxMessage> AddAsync(InboxMessage message, CancellationToken token)
    {
        try
        {
            await _context.InboxMessages.AddAsync(message, token);
            await _context.SaveChangesAsync(token);

            return message;
        }
        catch (Exception ex)
        {
            var error = "Ошибка добавления сообщения в БД";
            _logger.LogDebug(error, ex);
            throw new DbOperationException(error);
        }
    }
}
