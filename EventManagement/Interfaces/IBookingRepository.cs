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
    /// <param name="token">токен отмены</param>
    /// <returns>Добавленный объект брони</returns>
    Task<Booking> AddBookingAsync(Booking booking, CancellationToken token);

    /// <summary>
    /// Получить бронь по id
    /// </summary>
    /// <param name="id">id  брони</param>
    /// <param name="token">токен отмены</param>
    /// <returns>Объект брони</returns>
    Task<Booking?> GetBookingByIdAsync(Guid id, CancellationToken token);

    /// <summary>
    /// Получить брони в обработке
    /// </summary>
    /// <param name="token">токен отмены</param>
    /// <returns>Список броней со статусом Pending</returns>
    Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken token);
}