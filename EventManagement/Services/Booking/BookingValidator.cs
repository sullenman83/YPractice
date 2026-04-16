using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;

namespace EventManagement.Services;

/// <summary>
/// Валидатор заявки на бронирование
/// </summary>
/// <param name="eventRepository">Репозиторий с событиями</param>
public class BookingValidator(IEventRepository eventRepository) : IBookingValidator
{
    private readonly IEventRepository _eventRepository = eventRepository;

    /// <summary>
    /// Проверка есть ли событие с указанным id 
    /// </summary>
    /// <param name="eventId">id события</param>
    /// <param name="token">Токен отмены</param>
    /// <exception cref="BookingValidationException"></exception>
    public async Task ValidateAsync(Guid eventId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        //По идее тут в реальных условиях должен был бы быть асинхронный вызов получения записи из БД        
        if (_eventRepository.GetByID(eventId) == null)
            throw new BookingValidationException($"Не существует события с заданным id: {eventId}");
    }
}
