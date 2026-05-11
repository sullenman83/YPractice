using EventManagement.Data;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using Microsoft.EntityFrameworkCore;
using EventManagement.Extensions.EventExt;
using static System.Net.WebRequestMethods;

namespace EventManagement.Services;

/// <summary>
/// Хранилище данных
/// </summary>
public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="context"></param>
    public EventRepository(AppDbContext context)
    {
        _context = context;
    }
    ///<inheritdoc/>
    ///<exception cref="DbUpdateException">Ошибка при сохранении</exception>
    public async Task<Event> AddEventAsync(Event ev, CancellationToken token = default)
    {
        
        await _context.AddAsync(ev, token);
        await _context.SaveChangesAsync(token);

        return ev;
    }

    ///<inheritdoc/>
    public async Task<bool> DeleteEventAsync(Guid id, CancellationToken token = default)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(o => o.Id == id);
        if (ev == null)
            return false;
        _context.Remove(ev);
        await _context.SaveChangesAsync(token);

        return true;
    }

    ///<inheritdoc/>
    public async Task<Event?> GetEventByIDAsync(Guid id, CancellationToken token = default)
    {
         return await _context.Events.FirstOrDefaultAsync(o => o.Id == id, token);
    }

    ///<inheritdoc/>
    public async Task<IReadOnlyList<Event>> GetEventsAsync(EventFilterRequestDTO filter, CancellationToken token = default)
    {
        return await _context.Events
           .OrderBy(o => o.StartAt)
           .Filter(filter)
           .Paginate(filter)
           .ToArrayAsync();
    }

    ///<inheritdoc/>
    public async Task<int> GetEventsCountAsync(CancellationToken token = default)
    {
        return await _context.Events.CountAsync(token);
    }

    ///<inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken token)
    {
        await _context.SaveChangesAsync(token);
    }    
}
