using Bookings.Domain.Models;

namespace Bookings.Application.Interfaces.Repositories;

/// <summary>
/// Интерфейс хранилища бронирований событий
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Получить бронирование по id
    /// </summary>
    /// <param name="id">id бронирования</param>
    /// <param name="token">токен отмены</param>
    /// <returns>Бронирование</returns>
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken token = default);

    /// <summary>
    /// Добавить бронироние
    /// </summary>
    /// <param name="booking">Бронирование</param>
    /// <param name="token">токен отмены</param>
    /// <returns>Сохраненное бронирование</returns>
    Task<Booking> AddAsync(Booking booking, CancellationToken token = default);

    /// <summary>
    /// Получить брони в обработке
    /// </summary>
    /// <param name="token">токен отмены</param>
    /// <returns>Список броней со статусом Pending</returns>
    Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken token = default);
            
    ///// <summary>
    ///// Вернуть бронирование с мягкой блокировкой брони и события
    ///// </summary>
    ///// <param name="id">Идентификатор бронирования</param>
    ///// <param name="token">Токен отмены</param>
    ///// <returns>Бронирование</returns>
    ///// <exception cref="InvalidOperationException"></exception>
    //Task<Booking?> GetBookingWithBlockingAsync(Guid id, CancellationToken token = default);

    /// <summary>
    /// Получить активные бронирования 
    /// </summary>
    /// <param name="userId">id пользователя</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Список бронирований</returns>
    Task<List<Booking>> GetActiveUserBookingAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// Сохранить данные
    /// </summary>
    /// <param name="token">токен отмены</param>
    Task SaveChangesAsync(CancellationToken token = default);
}