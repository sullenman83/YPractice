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

        return new PaginatedResultDTO()
        {
            Events = events,
            EventsCount = events.Count,
            Page = filter.Page,
            EventsCountOnCurrentPage = events.Count
        };
    }

    ///<inheritdoc/>
    public async Task<Event?> GetEventWithBlockingAsync(Guid id, CancellationToken token)
    {
        if (_context.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Транзакция не открыта.");
            
        return await _context.Events.FromSql(
$@"SET LOCAL lock_timeout = '1s';
SELECT * FROM events WHERE id = {id} FOR UPDATE")
            .FirstOrDefaultAsync(token);
        
    }
}
