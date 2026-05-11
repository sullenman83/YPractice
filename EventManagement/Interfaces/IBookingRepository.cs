using EventManagement.Models.BookingModels;

namespace EventManagement.Interfaces;

/// <summary>
/// Интерфейс хранилища бронирований событий
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Добавить новую бронь
    /// </summary>
    /// <param name="booking">Объект брони</param>
    /// <returns>Добавленный объект брони</returns>
    Task<Booking> AddBookingAsync(Booking booking);

    /// <summary>
    /// Получить бронь по id
    /// </summary>
    /// <param name="id">id  брони</param>
    /// <returns>Объект брони</returns>
    Task<Booking> GetBookingByIdAsync(Guid id);

    /// <summary>
    /// Получить брони в обработке
    /// </summary>
    /// <returns>Список броней со статусом Pending</returns>
    Task<IReadOnlyList<Booking>> GetPendingBookingsAsync();
}