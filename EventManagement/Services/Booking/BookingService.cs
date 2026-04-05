using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;

namespace EventManagement.Services;

/// <summary>
/// Сервис для работы с заявками бронирования событий
/// </summary>
public class BookingService(IBookingRepository repository, IBookingValidator bookingValidator) : IBookingService
{
    private readonly IBookingRepository _repository = repository;
    private readonly IBookingValidator _bookingValidator = bookingValidator;

    /// <summary>
    /// Создать заявку на бронирование события
    /// </summary>
    /// <param name="eventId">Id события </param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Возвращает объект с описанием брони</returns>
    /// <exception cref="InvalidOperationException">Ошибка при создании нового бронирования.</exception>
    /// <exception cref="BookingValidationException">Ошибка при проверке броинрования.</exception>
    public async Task<BookingResponseDTO> CreateBookingAsync(Guid eventId, CancellationToken token)
    {
        await _bookingValidator.ValidateAsync(eventId, token);

        token.ThrowIfCancellationRequested();
        var booking = new Booking()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now
        };

        _repository.Bookings.TryAdd(booking.Id, booking);

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

        if (!_repository.Bookings.TryGetValue(bookingId, out var booking))
            throw new ArgumentException($"Не найдено бронирование с заданным id: {bookingId}");

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
