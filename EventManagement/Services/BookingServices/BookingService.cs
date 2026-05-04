using EventManagement.Common.Exceptions;
using EventManagement.Data;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using EventManagement.Models.BookingModels.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Services.BookingServices;

/// <summary>
/// Сервис для работы с заявками бронирования событий
/// </summary>
public class BookingService(AppDbContext dbContext) : IBookingService
{
    private readonly AppDbContext _dbContext = dbContext;
    private static readonly SemaphoreSlim _bookingLock = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Создать заявку на бронирование события
    /// </summary>
    /// <param name="eventId">Id события </param>
    /// <param name="seatsCount">Количество мест для бронирования</param> 
    /// <param name="token">Токен отмены</param>
    /// <returns>Возвращает объект с описанием брони</returns>
    /// <exception cref="InvalidOperationException">Ошибка при создании нового бронирования.</exception>
    /// <exception cref="NoAvailableSeatsException">Недостаточно мест для броинрования</exception>
    /// <exception cref="NotFoundException">Не найден объект</exception>
    /// <exception cref="ArgumentNullException">Неверные входные данные.</exception>
    /// <exception cref="ArgumentException">Неверные входные данные.</exception>
    public async Task<BookingResponseDTO> CreateBookingAsync(Guid eventId, int seatsCount, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        token.ThrowIfCancellationRequested();

        var booking = new Booking(BookingStatus.Pending, eventId, seatsCount, DateTimeOffset.UtcNow);

        await _bookingLock.WaitAsync(token);
        try
        {
            var ev = _dbContext.Events.FirstOrDefault(o => o.Id == eventId);
            if (ev == null)
                throw new NotFoundException($"Событие с id {eventId} не найдено в базе данных.");

            if (!ev.TryReserveSeats(seatsCount))
                throw new NoAvailableSeatsException("Нет доступных метс для бронирования");

            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();
        }
        finally
        {
            _bookingLock.Release();
        }

        return booking.ToResponse();
    }

    /// <summary>
    /// Вернуть бронирование по id
    /// </summary>
    /// <param name="bookingId">Иденификатор брони</param>
    /// <param name="token">Токен отмены</param>    
    /// <returns>Возвращает объект с описанием брони</returns>
    /// <exception cref="NotFoundException">Не найден объект</exception>
    /// <exception cref="ArgumentNullException">Неверные входные данные.</exception>
    public async Task<BookingResponseDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var booking = await _dbContext.Bookings.FirstOrDefaultAsync(o => o.Id == bookingId);
        if (booking == null)
            throw new NotFoundException($"Бронирование с id {bookingId} не найдено в базе данных.");
                
        return booking.ToResponse();
    }
}
