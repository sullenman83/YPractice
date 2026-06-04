using EventManagement.Application.Common.Exceptions;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace EventManagement.Infrastructure.Services.BookingServices;

/// <summary>
/// Класс хранения заявок на бронь
/// </summary>
public class BookingRepository(AppDbContext context, ILogger<BaseRepository<Booking>> logger) : BaseRepository<Booking>(context, logger), IBookingRepository<Booking>
{
    private const string LockRowError = "55P03";    

    ///<inheritdoc/>
    public async Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken token)
    {
        try
        {
            return await _context.Bookings
                .Where(o => o.Status == BookingStatus.Pending)
                .ToListAsync(token);
        }
        catch (Exception ex)
        {
            var message = "Ошибка чтения необработанных бронирований.";
            _logger.LogDebug(ex, message);
            throw new DbOperationException(message);
        }
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
            var message = "Ошибка плучения собыия с блокировкой";
            _logger.LogDebug(ex, message);
            
            if (ex.InnerException != null && ex.InnerException is PostgresException pex)
                if ( pex.SqlState == LockRowError)
                    throw new DbOperationWithBlockingRowException(message);

            throw new DbOperationException(message, ex);
        }
    }
}
