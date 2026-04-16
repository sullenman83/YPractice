using EventManagement.Models.BookingModels;

namespace EventManagement.Interfaces;

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
    /// <returns>Задача с пустым результатом</returns>
    Task<BookingResponseDTO> CreateBookingAsync(Guid eventId, int seatsCount, CancellationToken token);

    /// <summary>
    /// Вернуть бронирование по id
    /// </summary>
    /// <param name="bookingId">Иденификатор брони</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Возвращает объект с описанием брони</returns>
    Task<BookingResponseDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken token);
}
