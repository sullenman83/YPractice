using EventManagement.Data;
using EventManagement.Interfaces.Reposirories;
using EventManagement.Models.BookingModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        return await getPendingBookingsAsync(_context, token);
    }

    ///<inheritdoc/>
    public async Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(AppDbContext context, CancellationToken token)
    {
        return await getPendingBookingsAsync(context, token);
    }

    

    ///<inheritdoc/>
    public async Task<Booking?> GetBookingWithBlockingAsync(Guid id, CancellationToken token)
    {
        return await getBookingWithBlockingAsync(id, _context, token);
    }

    ///<inheritdoc/>
    public async Task<Booking?> GetBookingWithBlockingAsync(Guid id, AppDbContext context, CancellationToken token)
    {
        return await getBookingWithBlockingAsync(id, context, token);
    }


    /// <inheritdoc/>>    
    public async Task<IReadOnlyList<Booking>> GetProcessingBookingAsync(CancellationToken token = default)
    {
        return await getProcessingBookingAsync(_context, token);
    }

    /// <inheritdoc/>>    
    public async Task<IReadOnlyList<Booking>> GetProcessingBookingAsync(AppDbContext context, CancellationToken token = default)
    {
        return await getProcessingBookingAsync(context, token);
    }   

    private async Task<IReadOnlyList<Booking>> getPendingBookingsAsync(AppDbContext context, CancellationToken token)
    {
        return await context.Bookings
            .Where(o => o.Status == BookingStatus.Pending)
            .ToListAsync(token);
    }

    private async Task<Booking?> getBookingWithBlockingAsync(Guid id, AppDbContext context, CancellationToken token)
    {

        if (context.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Транзакция не открыта.");

        try
        {
            var result = await context.Bookings.FromSql(
    $@"SELECT b.*    
FROM bookings b 
JOIN events e ON e.id = b.event_id
WHERE b.id = {id}
FOR UPDATE")
                .Include(o => o.Event)
                .FirstOrDefaultAsync(token);

            return result;
        }
        catch (NpgsqlException ex)
        {
            throw new InvalidOperationException("Ошибка плучения собыия с блокировкой", ex);
        }
    }

    private async Task<IReadOnlyList<Booking>> getProcessingBookingAsync(AppDbContext context, CancellationToken token = default)
    {
        return await context.Bookings
            .Where(o => o.Status == BookingStatus.Processing)
            .ToListAsync(token);
    }
}
