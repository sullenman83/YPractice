using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventManagement.Interfaces.Reposirories;

/// <summary>
/// Интерфейс хранилища бронирований событий
/// </summary>
public interface IBookingRepository<T> :IBaseRepository<T>
{    

    /// <summary>
    /// Получить брони в обработке
    /// </summary>
    /// <param name="token">токен отмены</param>
    /// <returns>Список броней со статусом Pending</returns>
    Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken token = default);
        
    /// <summary>
    /// Вернуть бронирование с мягкой блокировкой брони и события
    /// </summary>
    /// <param name="id">Идентификатор бронирования</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Бронирование</returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<Booking?> GetBookingWithBlockingAsync(Guid id, CancellationToken token = default);
}