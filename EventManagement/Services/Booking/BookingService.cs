using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;

namespace EventManagement.Services;

/// <summary>
/// Сервис для работы с заявками бронирования событий
/// </summary>
public class BookingService(IBookingRepository repository) : IBookingService
{
    private readonly IBookingRepository _repository = repository;
    private readonly SemaphoreSlim _bookingLock = new (1, 1);

    /// <summary>
    /// Создать заявку на бронирование события
    /// </summary>
    /// <param name="eventId">Id события </param>
    /// <param name="seatsCount">Количество мест для бронирования</param> 
    /// <param name="token">Токен отмены</param>
    /// <returns>Возвращает объект с описанием брони</returns>
    /// <exception cref="InvalidOperationException">Ошибка при создании нового бронирования.</exception>
    public async Task<BookingResponseDTO> CreateBookingAsync(Guid eventId, int seatsCount, CancellationToken token)
    {        
        var booking = new Booking(BookingStatus.Pending, eventId)
        {
            CreatedAt = DateTime.Now
        };

        await _bookingLock.WaitAsync(token);
        
        token.ThrowIfCancellationRequested();



        _repository.Add(booking);

        return createBookingResponseDTO(booking);
    }

    /// <summary>
    /// Вернуть бронирование по id
    /// </summary>
    /// <param name="bookingId">Иденификатор брони</param>
    /// <param name="token">Токен отмены</param>    
    /// <returns>Возвращает объект с описанием брони</returns>
    /// <exception cref="ArgumentException">Не найдено бронирование с заданным id</exception>
    public async Task<BookingResponseDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var booking = _repository.GetById(bookingId);            

        return createBookingResponseDTO(booking);
    }

    private BookingResponseDTO createBookingResponseDTO(Booking booking)
    {
        return new BookingResponseDTO()
        {
            EventId = booking.EventId,
            Id = booking.Id,
            Status = booking.Status,
        };
    }
}
