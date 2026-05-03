using EventManagement.Common.Exceptions;
using EventManagement.Data;
using EventManagement.Extensions;
using EventManagement.Extensions.EventExt;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Models.Events.Extensions;
using EventManagement.Models.FilterModels;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Services.EventServices;

/// <summary>
/// Сервис для работы с событиями
/// </summary>
public class EventService(IEventValidator eventValidator, AppDbContext dbContext) : IEventService
{
    private readonly IEventValidator _eventValidator = eventValidator;
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Обновленное событие</returns>
    /// <exception cref="InvalidOperationException">Ошибка при создании нового события.</exception>
    /// <exception cref="ArgumentException">Ошибка при создании нового события.</exception>
    /// <exception cref="ArgumentNullException">Неверные входные данные.</exception>
    /// <exception cref="EventValidationException">Ошибка валидации</exception>    
    public async Task<EventResponseDto> CreateEventAsync(EventCreationDTO @event, CancellationToken token)
    {        
        await _eventValidator.ValidateAsync(@event, token);
         
        token.ThrowIfCancellationRequested();
        Event ev = @event.ToEvent();
        await _dbContext.Events.AddAsync(ev, token);
        await _dbContext.SaveChangesAsync(token);
                            
        return ev.ToResponse();
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">Идентификатор удаляемого события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <exception cref="NotFoundException">Не найдено событие с заданным id</exception>
    /// <exception cref="ArgumentNullException">Неверные входные данные.</exception>
    public async Task DeleteEventAsync(Guid id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var ev = await GetById(id);
        _dbContext.Events.Remove(ev);
        await _dbContext.SaveChangesAsync();
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
        var events = await _dbContext.Events
            .OrderBy(o => o.StartAt)
            .Filter(filter)
            .Paginate(filter)
            .Select(o => o.ToResponse())
            .ToListAsync(token);

        return new PaginatedResultDTO()
        {
            Events = events,
            EventsCount = _dbContext.Events.Count(),
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
    /// <exception cref="NotFoundException">Не найдено событие с заданным id</exception>
    /// <exception cref="ArgumentNullException">Неверные входные данные.</exception>
    public async Task<EventResponseDto> GetEventByIdAsync(Guid id, CancellationToken token)
    {
        var ev = await GetById(id);

        return  ev.ToResponse();
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="event">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Обновленное событие</returns>
    /// <exception cref="NotFoundException">Не найдено событие с заданным id</exception>
    /// <exception cref="ArgumentNullException">Неверные входные данные.</exception>
    /// <exception cref="EventValidationException">Ошибка валидации</exception>    
    public async Task<EventResponseDto> UpdateEventAsync(Guid id, EventUpdateDTO @event, CancellationToken token)
    {        
        await _eventValidator.ValidateAsync(@event, token);

        token.ThrowIfCancellationRequested();
        var ev = await GetById(id);
        _dbContext.Events.Update(ev.Update(@event));
        await _dbContext.SaveChangesAsync(token);

        return ev.ToResponse();
    }


    private async Task<Event> GetById(Guid id)
    {
        var ev = await _dbContext.Events.FirstOrDefaultAsync(o => o.Id == id);
        if (ev == null)
            throw new NotFoundException($"Событие с id {id} не найдено в базе данных.");

        return ev;
    }
}
