using Events.Application.Common.Exceptions;
using Events.Application.Interfaces.Repositories;
using Events.Application.Models.Messages;
using Events.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Events.Infrastructure.Services;

/// <summary>
/// Репозиторий входящих сообщений
/// </summary>
/// <param name="context">Контекст базы данных</param>
/// <param name="logger">Логгер</param>
public class InboxMessageRepository(AppDbContext context, ILogger<InboxMessageRepository> logger) : IInboxMessageRepository
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<InboxMessageRepository> _logger = logger;

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
