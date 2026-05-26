using EventManagement.Data;
using EventManagement.Interfaces.Reposirories;
using EventManagement.Models.BookingModels;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EventManagement.Services;

/// <summary>
/// Класс хранения заявок на бронь
/// </summary>
public class BookingRepository(AppDbContext context) : BaseRepository<Booking>(context), IBookingRepository<Booking>
{    
    ///<inheritdoc/>
    public async Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken token)
    {
        return await _context.Bookings
            .Where(o => o.Status == BookingStatus.Pending)
            .ToListAsync(token);
    }    

    ///<inheritdoc/>
    public async Task<Booking?> GetBookingWithBlockingAsync(Guid id, CancellationToken token)
    {
        if (_context.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Транзакция не открыта.");

        try
        {
            var result = await _context.Bookings.FromSql(
    $@"SELECT b.*    
FROM bookings b 
JOIN events e ON e.id = b.event_id
WHERE b.id = {id}
FOR UPDATE NOWAIT")
                .Include(o => o.Event)
                .FirstOrDefaultAsync(token);

            return result;
        }
        catch (NpgsqlException ex)
        {
            throw new InvalidOperationException("Ошибка плучения собыия с блокировкой", ex);
        }
    }
}
