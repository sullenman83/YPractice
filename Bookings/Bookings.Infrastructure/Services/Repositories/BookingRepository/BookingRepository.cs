using Bookings.Application.Exceptions;
using Bookings.Application.Interfaces.Repositories;
using Bookings.Domain.Models;
using Bookings.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookings.Infrastructure.Services.Repositories.BookingRepository;

/// <summary>
/// Класс хранения заявок на бронь
/// </summary>
public class BookingRepository(AppDbContext context, ILogger<BookingRepository> logger): IBookingRepository
{
    private readonly ILogger<BookingRepository> _logger = logger;
    private readonly AppDbContext _context = context;

    /// <inheritdoc/>
    public async Task<Booking> AddAsync(Booking booking, CancellationToken token = default)
    {
        try
        {
            await _context.Bookings.AddAsync(booking, token);
            await _context.SaveChangesAsync(token);

            return booking;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Ошибка добавления бронирования в БД  {id}", booking.Id);
            throw new DbOperationException("Ошибка добавления бронирования в БД");
        }
    }

    /// <inheritdoc/>
    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        try
        {
            return await _context.Bookings.FirstOrDefaultAsync(o => o.Id == id, token);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Ошибка получения бронирования по Id = {id}", id);
            throw new DbOperationException("Ошибка получения бронирования");
        }
    }

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
    public async Task<List<Booking>> GetActiveUserBookingAsync(Guid userId, CancellationToken token = default)
    {
        try
        {
            var bookings = _context.Bookings
                .Where(o => o.UserId == userId);

            return await bookings.Where(o => o.Status == BookingStatus.Pending
                || o.Status == BookingStatus.Confirmed)
                .ToListAsync(token);
        }        
        catch (Exception ex)
        {
            var message = "Ошибка чтения активных бронирований.";
            _logger.LogDebug(ex, message);
            throw new DbOperationException(message);
        }
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken token = default)
    {
        try
        {
            await _context.SaveChangesAsync(token);
        }
        catch (Exception ex)
        {
            var message = "Ошибка сохранения.";
            _logger.LogDebug(message, ex);
            throw new DbOperationException(message);
        }
    }
}
