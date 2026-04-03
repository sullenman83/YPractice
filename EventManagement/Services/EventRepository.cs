using EventManagement.Common;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using System.Collections.Concurrent;

namespace EventManagement.Services;

/// <summary>
/// Хранилище данных
/// </summary>
public class EventRepository : IEventRepository
{
    private readonly ConcurrentDictionary<Guid, Event> _repository = new (TestData.GetTestData());
        
    /// <summary>
    /// Коллекция событий
    /// </summary>
    public ConcurrentDictionary<Guid, Event> Data => _repository;
}
