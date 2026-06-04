using EventManagement.Application.Common.Exceptions;
using EventManagement.Application.Models.Events;
using EventManagement.Application.Models.FilterModels;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EventManagement.Infrastructure.Extensions.EventExt;
using EventManagement.Application.Models.Events.Extensions;
using EventManagement.Application.Interfaces.Repositories;

namespace EventManagement.Infrastructure.Services.EventServices;

/// <summary>
/// Хранилище данных
/// </summary>
public class EventRepository(AppDbContext context, ILogger<BaseRepository<Event>> logger) : BaseRepository<Event>(context, logger), IEventRepository<Event>
{
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
}
