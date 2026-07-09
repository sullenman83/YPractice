using Bookings.Domain.Models;

namespace Bookings.Application.Interfaces.BookingServices;

/// <summary>
/// Валидатор бронирований
/// </summary>
public interface IBookingValidator
{
    /// <summary>
    /// Проверить количество активных броней
    /// </summary>
    /// <param name="bookings">Список бронирований для события</param>
    void ValidateActiveBooking(IReadOnlyCollection<Booking> bookings);
}
