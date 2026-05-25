using EventManagement.Common.Exceptions;
using EventManagement.Data;
using EventManagement.Extensions.EventExt;
using EventManagement.Interfaces.Reposirories;
using EventManagement.Models.Events;
using EventManagement.Models.Events.Extensions;
using EventManagement.Models.FilterModels;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
}
