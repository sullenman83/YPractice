using EventManagement.Data;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

    ///<inheritdoc/>
    public async Task<int> GetEventsCountAsync(CancellationToken token)
    {
        return await _context.Events.CountAsync(token);
    }

    ///<inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken token)
    {
        await _context.SaveChangesAsync(token);
    }

    ///<inheritdoc/>
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken token)
    {
        return await _context.Database.BeginTransactionAsync(token);
    }

    ///<inheritdoc/>
    public async Task<Booking?> GetBookingWithBlockingAsync(Guid id, CancellationToken token)
    {
        if (_context.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Транзакция не открыта.");

        return await _context.Bookings.FromSql(
@$"SELECT b.*    
FROM bookings b 
JOIN events e ON e.id = b.event_id
WHERE b.id = {id}
FOR UPDATE")
            .Include(o => o.Event)
            .FirstOrDefaultAsync(token);
    }
}
