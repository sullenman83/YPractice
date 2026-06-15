namespace EventManagement.Domain.Models;

/// <summary>
/// Статус бронирования
/// </summary>
public enum BookingStatus
{
    /// <summary>
    /// Брони создана, ожидает обработки
    /// </summary>
    Pending,

    /// <summary>
    /// В обработке
    /// </summary>
    Processing,

    /// <summary>
    /// Бронь подтверждена
    /// </summary>
    Confirmed,

    /// <summary>
    /// бронь отклонена
    /// </summary>
    Rejected,

    /// <summary>
    /// Бронь отменена
    /// </summary>
    Cancelled

}
