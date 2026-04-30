namespace EventManagement.Models.BookingModels.Extensions;

/// <summary>
/// Класс расширение
/// </summary>
public static class BookingExtension
{
    /// <summary>
    /// Преобрабозовать бронь в объект BookingResponseDTO
    /// </summary>
    /// <param name="booking">Бронь</param>
    /// <returns>Объект BookingResponseDTO</returns>
    public static BookingResponseDTO ToResponse(this Booking booking)
    {
        return new BookingResponseDTO()
        {
            EventId = booking.EventId,
            Id = booking.Id,
            Status = booking.Status,
            SeatsCount = booking.SeatsCount,
            ProcessedAt = booking.ProcessedAt,
        };
    }
}
