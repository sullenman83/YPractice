using EventManagement.Data;
using EventManagement.Extensions.EventExt;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Models.Events.Extensions;
using EventManagement.Models.FilterModels;
using Microsoft.EntityFrameworkCore;

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
           .ToListAsync())
           .Select(o => o.ToResponse())
           .ToList();
        var cnt = _context.Events.Count();

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
        try
        {
            Console.WriteLine($"Старт {DateTime.Now}");
            if (_context.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Транзакция не открыта.");

            // ToDo: Потенциальное место для рефакторинга. Сделано по большей частью для тестов. Но может быть в каком-то виде применимо и для продакшен кода, чтобы запросы долго не висели в блокировке
            _context.Database.SetCommandTimeout(1);

            return await _context.Events.FromSql(
    $@"SELECT * FROM events WHERE id = {id} FOR UPDATE")
                .FirstOrDefaultAsync(token);
        }
        finally
        {
            Console.WriteLine($"Финиш {DateTime.Now}");
            _context.Database.SetCommandTimeout(null);
        }
    }
}
