using EventManagement.Data;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Services;

/// <summary>
/// Класс хранения заявок на бронь
/// </summary>
public class BookingRepository(AppDbContext context) : IBookingRepository
{
    private readonly AppDbContext _context = context;
    
    ///<inheritdoc/>
    public async Task<Booking> AddBookingAsync(Booking booking, CancellationToken token)
    {
        await _context.Bookings.AddAsync(booking, token);
        await _context.SaveChangesAsync(token);

        return booking;
    }

    ///<inheritdoc/>
    public async Task<Booking?> GetBookingByIdAsync(Guid id, CancellationToken token)
    {
        return await _context.Bookings.FirstOrDefaultAsync(o => o.Id == id, token);
    }

    ///<inheritdoc/>
    public async Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken token)
    {
        return await _context.Bookings
            .Where(o => o.Status == BookingStatus.Pending)
            .ToListAsync(token);
    }
}
