using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Services.BookingServices;

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

    /// <summary>
    /// Проверить, что событие еще не началось
    /// </summary>
    /// <param name="startDate">Дата начала события</param>
    void ValidateEventDate(DateTimeOffset startDate);
}
