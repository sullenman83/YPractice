using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;
using Microsoft.EntityFrameworkCore.Storage;

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
    Task<Booking> AddBookingAsync(Booking booking, CancellationToken token = default);

    /// <summary>
    /// Получить бронь по id
    /// </summary>
    /// <param name="id">id  брони</param>
    /// <param name="token">токен отмены</param>
    /// <returns>Объект брони</returns>
    Task<Booking?> GetBookingByIdAsync(Guid id, CancellationToken token = default);

    /// <summary>
    /// Получить брони в обработке
    /// </summary>
    /// <param name="token">токен отмены</param>
    /// <returns>Список броней со статусом Pending</returns>
    Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken token = default);

    /// <summary>
    /// Сохранить данные
    /// </summary>
    /// <param name="token">токун отмены</param>
    Task SaveChangesAsync(CancellationToken token);

    /// <summary>
    /// Создать транзакцию
    /// </summary>
    /// <param name="token">Токен оотмены</param>
    /// <returns>Транзакция</returns>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken token = default);

    /// <summary>
    /// Вернуть бронирование с мягкой блокировкой брони и события
    /// </summary>
    /// <param name="id">Идентификатор бронирования</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Бронирование</returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<Booking?> GetBookingWithBlockingAsync(Guid id, CancellationToken token = default);
}