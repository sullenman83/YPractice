
using Bookings.Application.Exceptions;
using Bookings.Application.Interfaces.Repositories;
using Bookings.Application.Models;
using Bookings.Domain.Models;
using Bookings.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Bookings.Infrastructure.Services.MessageRepositories;

/// <summary>
/// Репозиторий исходящих сообщений
/// </summary>
/// <param name="context">Контекст базы данных</param>
/// <param name="logger">Логгер</param>
public class OutboxMessageRepository(AppDbContext context, ILogger<OutboxMessageRepository> logger) : IOutboxMessageRepository
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<OutboxMessageRepository> _logger = logger;

    ///<inheritdoc/>
    public async Task<OutboxMessage> AddAsync(OutboxMessage message, CancellationToken token)
    {
        try
        {
            await _context.OutboxMessages.AddAsync(message, token);
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
