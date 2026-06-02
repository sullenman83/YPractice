using EventManagement.Application.Models.BookingModels;

namespace EventManagement.Application.Interfaces.Services;

/// <summary>
/// Интерфейс сервиса для работы с заявками бронирования событий
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Создать заявку на бронирование события
    /// </summary>
    /// <param name="eventId">Id события</param>
    /// <param name="seatsCount">Количество мест для бронирования</param> 
    /// <param name="token">Токен отмены</param>
    /// <returns>Возвращает объект с описанием брони</returns>
    Task<BookingResponseDTO> CreateBookingAsync(Guid eventId, int seatsCount, CancellationToken token = default);

    /// <summary>
    /// Вернуть бронирование по id
    /// </summary>
    /// <param name="bookingId">Иденификатор брони</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Возвращает объект с описанием брони</returns>
    Task<BookingResponseDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken token = default);
}
