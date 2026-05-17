using EventManagement.Models.BookingModels;

namespace EventManagement.Models.Data;

/// <summary>
/// Бронирование (сущность БД)
/// </summary>
public class BookingEntity
{
    /// <summary>
    /// Идентификатор бронирования
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Текущий статус брони
    /// </summary>
    public BookingStatus Status { get; set; }

    /// <summary>
    /// Кр=оличество мест в брони
    /// </summary>
    public int SeatsCount { get; set; }

    /// <summary>
    /// Дата и время создания брони
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Дата и время обработки брони
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; set; }
}
