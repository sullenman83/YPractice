using EventManagement.Application.Interfaces.Reposirories;
using EventManagement.Application.Models.Events;
using EventManagement.Application.Models.FilterModels;
using EventManagement.Common.Exceptions;
using EventManagement.Extensions.EventExt;
using EventManagement.Infrastructure.Data;
using EventManagement.Models.Events.Extensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EventManagement.Infrastructure.Services.EventServices;

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
