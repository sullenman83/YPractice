using Events.Application.Common;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Interfaces.Repositories;
using Events.Application.Interfaces.Validators;
using Events.Application.Models;
using Events.Application.Models.Extensions;
using Events.Application.Models.FilterModels;
using Events.Application.Settings;
using Events.Domain.Exceptions;
using Events.Domain.Models;
using Microsoft.Extensions.Options;
namespace Events.Application.Services;

/// <summary>
/// Сервис для работы с событиями
/// </summary>
public class EventService : IEventService
{
    private readonly IEventValidator _eventValidator ;
    private readonly IEventRepository _eventRepository;
    private readonly ICacheService _cacheService;    
    private readonly TimeSpan _eventTTL;
    private readonly TimeSpan _top10TTL;

    /// <summary>
    /// конструктор
    /// </summary>
    /// <param name="eventValidator">валидатор событий</param>
    /// <param name="eventRepository">Репозиторий событий</param>
    /// <param name="cacheService">Кеш</param>>
    /// <param name="ttlSettings">настройки TTL</param>
    /// <exception cref="InvalidOperationException"></exception>
    public EventService(IEventValidator eventValidator, IEventRepository eventRepository, ICacheService cacheService, IOptions<TTLSettings> ttlSettings)
    {
        _eventValidator = eventValidator;
        _eventRepository = eventRepository;
        _cacheService = cacheService;
        var ttl = ttlSettings.Value ?? throw new InvalidOperationException("Не заданы настройки TTL");
        _eventTTL = TimeSpan.FromSeconds(ttl.EventTTL);
        _top10TTL = TimeSpan.FromSeconds(ttl.Top10TTL);
    }

    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Обновленное событие</returns>
    /// <exception cref="DbOperationException">Ошибка операций с БД.</exception>
    /// <exception cref="OperationCanceledException">Операция отменена.</exception>    
    /// <exception cref="EventValidationException">Ошибка валидации</exception>    
    public async Task<EventResponseDto> CreateEventAsync(EventCreationDTO @event, CancellationToken token)
    {
        _eventValidator.Validate(@event);

        token.ThrowIfCancellationRequested();
        Event ev = @event.ToEvent();
        ev = await _eventRepository.AddAsync(ev, token);
                            
        return ev.ToResponse();
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">Идентификатор удаляемого события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <exception cref="NotFoundException">Не найдено событие с заданным id</exception>
    /// <exception cref="DbOperationException">Ошибка операций с БД.</exception>
    /// <exception cref="OperationCanceledException">Операция отменена.</exception>    
    public async Task DeleteEventAsync(Guid id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        
        if (!await _eventRepository.DeleteAsync(id, token))
        {
            throw new NotFoundException($"Не найдено событие с id = {id}");
        }
        await _cacheService.DeleteAsync(CacheKeys.EventKey(id));
    }

    /// <summary>
    /// Получить все события
    /// </summary>
    /// <param name="filter">Фильтр событий</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Отфильтрованный список событий по страницам</returns>
    /// <exception cref="OperationCanceledException">Операция отменена.</exception>    
    /// <exception cref="DbOperationException">Ошибка операций с БД.</exception>
    public async Task<PaginatedResultDTO> GetEventsAsync(EventFilterRequestDTO filter, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _eventRepository.GetEventsByFilterAsync(filter, token);
    }

    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Событие с искомым идентификатором</returns>
    /// <exception cref="NotFoundException">Не найдено событие с заданным id</exception>
    /// <exception cref="OperationCanceledException">Операция отменена.</exception>
    /// <exception cref="DbOperationException">Ошибка операций с БД.</exception>
    public async Task<EventResponseDto> GetEventByIdAsync(Guid id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var eventResponse = await _cacheService.GetAsync<EventResponseDto>(CacheKeys.EventKey(id));
        if (eventResponse != null)
            return eventResponse;

        var ev = await _eventRepository.GetByIdAsync(id, token);
        if (ev == null)
            throw new NotFoundException($"Не найдено событие с id = {id}");

        var response = ev.ToResponse();
        await _cacheService.SetAsync(CacheKeys.EventKey(id), response, _eventTTL);

        return response;
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="ev">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Обновленное событие</returns>
    /// <exception cref="NotFoundException">Не найдено событие с заданным id</exception>
    /// <exception cref="DbOperationException">Ошибка операций с БД.</exception>
    /// <exception cref="EventValidationException">Ошибка валидации</exception>    
    /// <exception cref="OperationCanceledException">Операция отменена.</exception>    
    /// <exception cref="ArgumentNullException">Неверные входные данные.</exception>
    /// <exception cref="ArgumentException">Неверные входные данные.</exception>
    public async Task<EventResponseDto> UpdateEventAsync(Guid id, EventUpdateDTO ev, CancellationToken token)
    {        
        _eventValidator.Validate(ev);

        token.ThrowIfCancellationRequested();

        var e = await _eventRepository.GetByIdAsync(id, token);
        if (e == null)
            throw new NotFoundException($"Не найдено событие с id = {id}");
        
        e.Title = ev.Title;
        e.Description = ev.Description;
        e.StartAt = ev.StartAt.HasValue ? ev.StartAt.Value : throw new ArgumentNullException("Дата начала события должна быть заполнена");
        e.EndAt = ev.EndAt.HasValue ? ev.EndAt.Value : throw new ArgumentNullException("Дата окончания события должна быть заполнена");
                
        await _eventRepository.SaveChangesAsync(token);

        await _cacheService.DeleteAsync(CacheKeys.EventKey(id));

        return e.ToResponse();
    }

    ///<inheritdoc/>
    ///<exception cref="DbOperationException">Ошибка операций с БД.</exception>
    public async Task<List<EventResponseDto>> GetTop10Events(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var list = await _cacheService.GetAsync<List<EventResponseDto>>(CacheKeys.Top10EventsKey());
        if (list != null)
            return list;

        var res = await _eventRepository.GetTopEvents(10, token);

        list = res.Select(o => o.ToResponse())
            .ToList();

        await _cacheService.SetAsync<List<EventResponseDto>>(CacheKeys.Top10EventsKey(), list, _top10TTL);

        return list;
    }    
}
