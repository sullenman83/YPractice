namespace Events.Application.Interfaces.Validators;

/// <summary>
/// Интерфейс проверки события на дату начала. Событие не должно начаться на момент подтвержения
/// </summary>
public interface IBookingConfirmedValidator
{
    /// <summary>
    /// Проверить, что событие еще не началось
    /// </summary>
    /// <param name="startDate">Дата начала события</param>
    void ValidateEventDate(DateTimeOffset startDate);
}
