using EventManagement.Models.BookingModels;
using System.Collections.Concurrent;

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
    Booking Add(Booking booking);

    /// <summary>
    /// Обновить бронь
    /// </summary>
    /// <param name="booking">Объект брони</param>
    /// <returns>Измененный объект брони</returns>
    Booking Update(Booking booking);

    /// <summary>
    /// Получить бронь по id
    /// </summary>
    /// <param name="id">id  брони</param>
    /// <returns>Объект брони</returns>
    Booking GetById(Guid id);

    /// <summary>
    /// Получить брони в обработке
    /// </summary>
    /// <returns>Список броней с о статусоь Pending</returns>
    IEnumerable<Booking> GetPending();
}
