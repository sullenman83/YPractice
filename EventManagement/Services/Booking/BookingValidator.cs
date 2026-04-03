using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;

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
        //По идее тут в реальных условиях должен был бы быть асинхронных выхов полкучения записи из БД        
        if (!_eventRepository.Data.TryGetValue(eventId, out var result))
            throw new BookingValidationException($"Не существует события с заданным id: {eventId}");
    }
}
