using EventManagement.Data;
using EventManagement.Interfaces.Reposirories;
using EventManagement.Models.BookingModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

        // ToDo: Потенциальное место для рефакторинга. Сделано по большей частью для тестов. Но может быть в каком-то виде применимо и для продакшен кода, чтобы запросы долго не висели в блокировке            
        return await _context.Bookings.FromSql(
@$"SET LOCAL lock_timeout = '1s';
SELECT b.*    
FROM bookings b 
JOIN events e ON e.id = b.event_id
WHERE b.id = {id}
FOR UPDATE")
            .Include(o => o.Event)
            .FirstOrDefaultAsync(token);
    }        
}
