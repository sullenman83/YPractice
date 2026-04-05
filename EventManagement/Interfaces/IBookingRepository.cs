using EventManagement.Models.BookingModels;
using System.Collections.Concurrent;

namespace EventManagement.Interfaces;

/// <summary>
/// Интерфейс хранилища бронирований событий
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Хранилище броней
    /// </summary>
    ConcurrentDictionary<Guid, Booking> Bookings { get; }
}
