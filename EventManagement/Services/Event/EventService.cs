using EventManagement.Extensions;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;

namespace EventManagement.Services;

/// <summary>
/// Сервис для работы с событиями
/// </summary>
public class EventService(IEventValidator eventValidator, IEventRepository repository) : IEventService
{
    private readonly IEventValidator _eventValidator = eventValidator;
    private readonly IEventRepository _repository = repository;

    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Обновленное событие</returns>
    /// <exception cref="InvalidOperationException">Ошибка при создании нового события.</exception>
    public async Task<EventResponseDto> CreateEventAsync(EventRequestDto @event, CancellationToken token)
    {        
        await _eventValidator.Validate(@event, token);
         
        token.ThrowIfCancellationRequested();
        var ev = createEvent(@event);
        _repository.Data.TryAdd(ev.Id, ev);            
                            
        return createEventResponseDto(ev);
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">Идентификатор удаляемого события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    public async Task DeleteEventAsync(Guid id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!_repository.Data.TryRemove(id, out var ev))
            throw new ArgumentException($"Ошбика при удалении события {id}.");
    }

    /// <summary>
    /// Получить все события
    /// </summary>
    /// <param name="filter">Фильтр событий</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Список событий</returns>
    public async Task<PaginatedResultDTO> GetEventsAsync(EventFilterRequestDTO filter, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var events = _repository.Data.ToArray()
            .Select(o => o.Value)
            .OrderBy(o => o.StartAt)
            .Filter(filter)
            .Paginate(filter)
            .Select(o => createEventResponseDto(o))
            .ToList();

        return new PaginatedResultDTO()
        {
            Events = events,
            EventsCount = _repository.Data.Count,
            Page = filter.Page,
            EventsCountOnCurrentPage = events.Count
        };
    }

    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Событие с искомым идентификатором</returns>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    public async Task<EventResponseDto> GetEventByIdAsync(Guid id, CancellationToken token)
    {
        if (!_repository.Data.TryGetValue(id, out var ev))
            throw new ArgumentException($"Ошбика при получении события по {id}.");

        return  createEventResponseDto(ev);
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="event">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Обновленное событие</returns>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    public async Task<EventResponseDto> UpdateEventAsync(Guid id, EventRequestDto @event, CancellationToken token)
    {        
        await _eventValidator.Validate(@event, token);

        token.ThrowIfCancellationRequested();
        var ev = getEventById(id);
        var newEv = ev.Clone() as Event;

        updateEvent(@event, newEv);
        if (!_repository.Data.TryUpdate(id, newEv, ev))
            throw new ArgumentException($"Ошбика при обновлении события по {id}.");

        return createEventResponseDto(newEv);
    }

    private Event createEvent(EventRequestDto source)
    {
        return new Event()
        {
            Id = Guid.NewGuid(),
            Title = source.Title,
            Description = source.Description,
            StartAt = source.StartAt,
            EndAt = source.EndAt,
        };
    }

    private EventResponseDto createEventResponseDto(Event source)
    {
        return new EventResponseDto()
        {
            Id = source.Id,
            Title = source.Title,
            Description = source.Description,
            StartAt = source.StartAt,
            EndAt = source.EndAt,
        };
    }

    private void updateEvent(EventRequestDto source, Event dest)
    {
        dest.EndAt = source.EndAt;
        dest.StartAt = source.StartAt;
        dest.Title = source.Title;
        dest.Description = source.Description;        
    }

    private Event getEventById(Guid id)
    {
        if (!_repository.Data.TryGetValue(id, out var ev))
            throw new ArgumentException($"Не найдено событие с id = {id}");
        
        return ev!;
    }
}
