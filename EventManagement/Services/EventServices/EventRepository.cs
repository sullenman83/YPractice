using EventManagement.Common;
using EventManagement.Common.Exceptions;
using EventManagement.Data;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using System.Collections.Concurrent;

namespace EventManagement.Services;

/// <summary>
/// Хранилище данных
/// </summary>
public class EventRepository : IEventRepository
{
    ///<inheritdoc/>
    public Task<Event> AddEventAsync(Event ev)
    {
        throw new NotImplementedException();
    }

    ///<inheritdoc/>
    public Task<Event> DeleteEventAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    ///<inheritdoc/>
    public Task<Event> GetEventByIDAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    ///<inheritdoc/>
    public Task<IReadOnlyList<Event>> GetEventsAsync()
    {
        throw new NotImplementedException();
    }

    ///<inheritdoc/>
    public Task<int> GetEventsCountAsync()
    {
        throw new NotImplementedException();
    }

    ///<inheritdoc/>
    public Task<Event> UpdateEventAsync(Event ev)
    {
        throw new NotImplementedException();
    }
}
