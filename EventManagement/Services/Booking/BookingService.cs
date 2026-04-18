using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using EventManagement.Models.BookingModels.Extensions;

namespace EventManagement.Services;

/// <summary>
/// Сервис для работы с заявками бронирования событий
/// </summary>
public class BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository) : IBookingService
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly SemaphoreSlim _bookingLock = new (1, 1);

    /// <summary>
    /// Создать заявку на бронирование события
    /// </summary>
    /// <param name="eventId">Id события </param>
    /// <param name="seatsCount">Количество мест для бронирования</param> 
    /// <param name="token">Токен отмены</param>
    /// <returns>Возвращает объект с описанием брони</returns>
    /// <exception cref="InvalidOperationException">Ошибка при создании нового бронирования.</exception>
    /// <exception cref="NoAvailableSeatsException">Недостаточно мест для броинрования</exception>
    public async Task<BookingResponseDTO> CreateBookingAsync(Guid eventId, int seatsCount, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var booking = new Booking(BookingStatus.Pending, eventId, DateTime.Now);

        await _bookingLock.WaitAsync(token);        
        try
        {
            var ev = _eventRepository.GetByID(eventId);
            if (!ev.TryReserveSeats(seatsCount))
                throw new NoAvailableSeatsException("Нет доступных метс для бронирования");
                        
            booking = _bookingRepository.Add(booking);
            _eventRepository.Update(ev);
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
    /// <exception cref="ArgumentException">Не найдено бронирование с заданным id</exception>
    public async Task<BookingResponseDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var booking = _bookingRepository.GetById(bookingId);            

        return booking.ToResponse();
    }
}
