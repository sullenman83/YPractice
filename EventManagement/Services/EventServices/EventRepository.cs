using EventManagement.Data;
using EventManagement.Extensions.EventExt;
using EventManagement.Interfaces.Reposirories;
using EventManagement.Models.Events;
using EventManagement.Models.Events.Extensions;
using EventManagement.Models.FilterModels;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EventManagement.Services;

/// <summary>
/// Хранилище данных
/// </summary>
public class EventRepository(AppDbContext context) : BaseRepository<Event>(context), IEventRepository<Event>
{
    ///<inheritdoc/>
    public async Task<PaginatedResultDTO> GetEventsByFilterAsync(EventFilterRequestDTO filter, CancellationToken token)
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

    ///<inheritdoc/>
    public async Task<Event?> GetEventWithBlockingAsync(Guid id, CancellationToken token)
    {
        if (_context.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Транзакция не открыта.");

        await _context.Database.ExecuteSqlRawAsync(
$@"SET LOCAL lock_timeout = '1s'");
        
        var result = await _context.Events.FromSql(
$@"SELECT * FROM events WHERE id = {id} FOR UPDATE")
            .FirstOrDefaultAsync(token);

        await _context.Database.ExecuteSqlRawAsync(
$@"SET LOCAL lock_timeout = '0'");

        return result;
    }
}
