using EventManagement.Data;
using EventManagement.Extensions.EventExt;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventManagement.Services;

/// <summary>
/// Хранилище данных
/// </summary>
public class EventRepository(AppDbContext context) : BaseRepository<Event>(context), IEventRepository<Event>
{
    
    ///<inheritdoc/>
    public async Task<IReadOnlyList<Event>> GetEventsByFilterAsync(EventFilterRequestDTO filter, CancellationToken token)
    {
        return await _context.Events
           .OrderBy(o => o.StartAt)
           .Filter(filter)
           .Paginate(filter)
           .ToListAsync();
    }

    ///<inheritdoc/>
    public async Task<Event?> GetEventWithBlockingAsync(Guid id, CancellationToken token)
    {
        try
        {
            if (_context.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Транзакция не открыта.");

            // ToDo: Потенциальное место для рефакторинга. Сделано по большей частью для тестов. Но может быть в каком-то виде применимо и для продакшен кода, чтобы запросы долго не висели в блокировке
            _context.Database.SetCommandTimeout(TimeSpan.FromMilliseconds(200));

            return await _context.Events.FromSql(
    $@"SELECT * FROM events WHERE id = {id} FOR UPDATE")
                .FirstOrDefaultAsync(token);
        }
        finally
        {
            _context.Database.SetCommandTimeout(null);
        }
    }
}
