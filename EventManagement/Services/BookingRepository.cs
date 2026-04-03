using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using System.Collections.Concurrent;

namespace EventManagement.Services;

/// <summary>
/// Класс хранения заявок на бронь
/// </summary>
public class BookingRepository : IBookingRepository
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();

    /// <summary>
    /// Коллекция заявок на бронь
    /// </summary>
    public ConcurrentDictionary<Guid, Booking> Bookings => _bookings;
}
