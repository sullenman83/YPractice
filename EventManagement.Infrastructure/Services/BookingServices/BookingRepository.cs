using EventManagement.Application.Interfaces.Reposirories;
using EventManagement.Application.Models.BookingModels;
using EventManagement.Common.Exceptions;
using EventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EventManagement.Infrastructure.Services.BookingServices;

/// <summary>
/// Класс хранения заявок на бронь
/// </summary>
public class BookingRepository(AppDbContext context) : BaseRepository<Booking>(context), IBookingRepository<Booking>
{
    private const string LockRowError = "55P03";

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
        catch (Exception ex)
        {
            if (ex.InnerException != null && ex.InnerException is PostgresException pex)

            if ( pex.SqlState == LockRowError)
                throw new DbOperationWithBlockingRowException("Не удалось полуить записи бронирования с блокировкой.");

            throw new InvalidOperationException("Ошибка плучения собыия с блокировкой", ex);
        }
    }
}
