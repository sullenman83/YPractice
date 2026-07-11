using Events.Application;
using Events.Application.Models;
using Events.Application.Models.Extensions;
using Events.Application.Models.FilterModels;
using Events.Domain.Models;
using Events.Infrastructure.Common;
using Events.Infrastructure.Data;
using Events.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Events.Application.Exceptions;
using Events.Application.Interfaces.Repositories;

namespace Events.Infrastructure.Services;

/// <summary>
/// Хранилище данных
/// </summary>
public class EventRepository(AppDbContext context, ILogger<EventRepository> logger): IEventRepository
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<EventRepository> _logger = logger;

    /// <inheritdoc/>
    public async Task<Event> AddAsync(Event ev, CancellationToken token = default)
    {
        try
        {
            await _context.Events.AddAsync(ev, token);
            await _context.SaveChangesAsync(token);

            return ev;
        }
        catch (Exception ex)
        {
            var message = "Ошибка добавления события в БД";
            _logger.LogDebug(message, ex);
            throw new DbOperationException(message);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
    {
        try
        {
            var ev = await _context.Events.FirstOrDefaultAsync(o => o.Id == id, token);
            if (ev == null)
                return false;
            _context.Remove(ev);
            await _context.SaveChangesAsync(token);

            return true;
        }
        catch (Exception ex)
        {
            var message = $"Ошибка удаления события {id} из БД";
            _logger.LogDebug(message, ex);
            throw new DbOperationException(message);
        }
    }

    /// <inheritdoc/>
    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        try
        {
            return await _context.Events.FirstOrDefaultAsync(o => o.Id == id, token);
        }
        catch (Exception ex)
        {
            var message = $"Ошибка получения события по Id = {id}";
            _logger.LogDebug(message, ex);
            throw new DbOperationException(message);
        }
    }

    ///// <inheritdoc/>
    //public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken token = default)
    //{
    //    try
    //    {
    //        return await _context.Events.ToListAsync(token);
    //    }
    //    catch (Exception ex)
    //    {
    //        var message = "Ошибка чтения событий.";
    //        _logger.LogDebug(message, ex);
    //        throw new DbOperationException(message);
    //    }
    //}

    ///// <inheritdoc/>
    //public async Task<int> GetCountAsync(CancellationToken token = default)
    //{
    //    try
    //    {
    //        return await _context.Events.CountAsync(token);
    //    }
    //    catch (Exception ex)
    //    {
    //        var message = "Ошибка получения количества событий.";
    //        _logger.LogDebug(message, ex);
    //        throw new DbOperationException(message);
    //    }
    //}

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken token = default)
    {
        try
        {
            await _context.SaveChangesAsync(token);
        }
        catch (Exception ex)
        {
            var message = "Ошибка сохранения.";
            _logger.LogDebug(message, ex);
            throw new DbOperationException(message);
        }
    }

    ///<inheritdoc/>
    public async Task<PaginatedResultDTO> GetEventsByFilterAsync(EventFilterRequestDTO filter, CancellationToken token)
    {
        try
        {
            var events = (await _context.Events
               .OrderBy(o => o.StartAt)
               .Filter(filter)
               .Paginate(filter)
               .ToListAsync(token))
               .Select(o => o.ToResponse())
               .ToList();

            var cnt = await _context.Events
               .OrderBy(o => o.StartAt)
               .Filter(filter)
               .CountAsync(token);

            return new PaginatedResultDTO()
            {
                Events = events,
                EventsCount = cnt,
                Page = filter.Page,
                EventsCountOnCurrentPage = events.Count
            };
        }
        catch (Exception ex)
        {
            var message = "Ошибка полечения событий с фильтром";
            _logger.LogError(message, ex);
            throw new DbOperationException(message);
        }
    }

    /////<inheritdoc/>
    //public async Task<Event?> GetEventWithBlockingAsync(Guid id, CancellationToken token)
    //{
    //    if (_context.Database.CurrentTransaction == null)
    //        throw new InvalidOperationException("Транзакция не открыта.");
    //    try
    //    {
    //        var result = await _context.Events.FromSql(
    //$@"SELECT * FROM events WHERE id = {id} FOR UPDATE NOWAIT")
    //            .FirstOrDefaultAsync(token);

    //        return result;
    //    }
    //    catch (Exception ex)
    //    {
    //        var message = "Ошибка плучения собыия с блокировкой";
    //        _logger.LogDebug(ex, message);

    //        if (ex.InnerException != null && ex.InnerException is PostgresException pex)
    //            if (pex.SqlState == DbErrorCodes.LockRowError)
    //                throw new DbOperationWithBlockingRowException(message);

    //        throw new DbOperationException(message, ex);
    //    }
    //}
}
