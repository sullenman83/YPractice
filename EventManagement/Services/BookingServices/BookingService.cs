using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using EventManagement.Models.BookingModels.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Services.BookingServices;

/// <summary>
/// Сервис для работы с заявками бронирования событий
/// </summary>
public class BookingService(IBookingRepository bookingRepository, IEventRepository eventRepoository) : IBookingService
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IEventRepository _eventRepository = eventRepoository;

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
    /// <exception cref="OperationCanceledException">Операция отменена</exception>
    public async Task<BookingResponseDTO> CreateBookingAsync(Guid eventId, int seatsCount, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var booking = new Booking(BookingStatus.Pending, eventId, seatsCount, DateTimeOffset.UtcNow);
        
        try
        {
            using var tr = await _eventRepository.BeginTransactionAsync(token);
            var ev = await _eventRepository.GetEventWithBlockingAsync(eventId, token);
            if (ev == null)
                throw new NotFoundException($"Событие с id {eventId} не найдено в базе данных.");

            if (!ev.TryReserveSeats(seatsCount))
                throw new NoAvailableSeatsException("Нет доступных метс для бронирования");

            await _bookingRepository.AddBookingAsync(booking, token);
            await _eventRepository.SaveChangesAsync(token);
            await tr.CommitAsync();

            return booking.ToResponse();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Ошибка при создании бронирования.", ex);
        }
    }

    /// <summary>
    /// Вернуть бронирование по id
    /// </summary>
    /// <param name="bookingId">Иденификатор брони</param>
    /// <param name="token">Токен отмены</param>    
    /// <returns>Возвращает объект с описанием брони</returns>
    /// <exception cref="NotFoundException">Не найден объект</exception>
    /// <exception cref="OperationCanceledException">Операция отменена</exception>
    public async Task<BookingResponseDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, token);
        if (booking == null)
            throw new NotFoundException($"Бронирование с id {bookingId} не найдено в базе данных.");
                
        return booking.ToResponse();
    }
}
