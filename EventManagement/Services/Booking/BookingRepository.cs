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
    /// Добавить новую бронь
    /// </summary>
    /// <param name="booking">Объект брони</param>
    public void Add(Booking booking)
    {
        if (!_bookings.TryAdd(booking.Id, booking))
            throw new InvalidOperationException("Ошибка при добавлении брони.");
    }

    /// <summary>
    /// Обновить бронь
    /// </summary>
    /// <param name="booking">Объект брони</param>
    public void Update(Booking booking)
    {
        if (!_bookings.TryGetValue(booking.Id, out var oldBooking))
            throw new ArgumentException("Ошибка при получении брони по id.");

        if (!_bookings.TryUpdate(booking.Id, booking, oldBooking))
            throw new InvalidOperationException("Ошибка при обновлении брони.");
    }

    /// <summary>
    /// Получить бронь по id
    /// </summary>
    /// <param name="id">id  брони</param>
    /// <returns>Объект брони</returns>
    public Booking GetById(Guid id)
    { 
        if (!_bookings.TryGetValue(id, out var booking))
            throw new ArgumentException("Ошибка при получении брони по id.");
        
        return booking.Clone();
    }

    /// <summary>
    /// Получить брони в обработке
    /// </summary>
    /// <returns>Список броней</returns>
    public IEnumerable<Booking> GetPending()
    {
        return _bookings.Where(o => o.Value.Status == BookingStatus.Pending)
            .Select(o => o.Value.Clone());
    }
}
