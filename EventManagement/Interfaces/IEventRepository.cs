using EventManagement.Models.Events;
using System.Collections.Concurrent;

namespace EventManagement.Interfaces;

/// <summary>
/// Хранилище данных
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Коллекция событий
    /// </summary>
    ConcurrentDictionary<int, Event> Data { get; }
}
