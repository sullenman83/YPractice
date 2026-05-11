using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using System.Collections.Concurrent;

namespace EventManagement.Services;

/// <summary>
/// Класс хранения заявок на бронь
/// </summary>
public class BookingRepository : IBookingRepository
{
    ///<inheritdoc/>
    ///<exception cref="NotImplementedException">Не реализован</exception>
    public Task<Booking> AddBookingAsync(Booking booking)
    {
        throw new NotImplementedException();
    }

    ///<inheritdoc/>
    public Task<Booking> GetBookingByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    ///<inheritdoc/>
    public Task<IReadOnlyList<Booking>> GetPendingBookingsAsync()
    {
        throw new NotImplementedException();
    }
}
