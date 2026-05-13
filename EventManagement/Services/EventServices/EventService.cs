using EventManagement.Common.Exceptions;
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
public class EventService(IEventValidator eventValidator, IEventRepository eventRepository) : IEventService
{
    private readonly IEventValidator _eventValidator = eventValidator;
    private readonly IEventRepository _eventRepository = eventRepository;

    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Обновленное событие</returns>
    /// <exception cref="InvalidOperationException">Ошибка при создании нового события.</exception>
    /// <exception cref="OperationCanceledException">Операция отменена.</exception>    
    /// <exception cref="EventValidationException">Ошибка валидации</exception>    
    public async Task<EventResponseDto> CreateEventAsync(EventCreationDTO @event, CancellationToken token)
    {
        await _eventValidator.ValidateAsync(@event, token);

        token.ThrowIfCancellationRequested();
        Event ev = @event.ToEvent();

        try
        {
            ev = await _eventRepository.AddEventAsync(ev, token);
        }                                
        catch(DbUpdateException ex)
        {
            throw new InvalidOperationException($"Ошибка при создании события", ex);
        }
                            
        return ev.ToResponse();
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">Идентификатор удаляемого события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <exception cref="NotFoundException">Не найдено событие с заданным id</exception>
    /// <exception cref="InvalidOperationException">Ошибка при удалении события.</exception>
    /// <exception cref="OperationCanceledException">Операция отменена.</exception>    
    public async Task DeleteEventAsync(Guid id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        try
        {
            if (!await _eventRepository.DeleteEventAsync(id, token))
            {
                throw new NotFoundException($"Не найдено событие с id = {id}");
            }
        } 
        catch(DbUpdateException ex)
        {
            throw new InvalidOperationException($"Ошибка при удалении события id = {id}", ex);
        }
    }

    /// <summary>
    /// Получить все события
    /// </summary>
    /// <param name="filter">Фильтр событий</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Ттфильтрованный список событий по страницам</returns>
    /// <exception cref="OperationCanceledException">Операция отменена.</exception>    
    public async Task<PaginatedResultDTO> GetEventsAsync(EventFilterRequestDTO filter, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var events = (await _eventRepository.GetEventsAsync(filter, token))
            .Select(o => o.ToResponse())
            .ToList();
        var cnt = await  _eventRepository.GetEventsCountAsync(token);

        return new PaginatedResultDTO()
        {
            Events = events,
            EventsCount = cnt,
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
    /// <exception cref="OperationCanceledException">Операция отменена.</exception>    
    public async Task<EventResponseDto> GetEventByIdAsync(Guid id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var ev = await GetById(id, token);

        return  ev.ToResponse();
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="ev">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Обновленное событие</returns>
    /// <exception cref="NotFoundException">Не найдено событие с заданным id</exception>
    /// <exception cref="InvalidOperationException">Ошибка при удалении события.</exception>
    /// <exception cref="EventValidationException">Ошибка валидации</exception>    
    /// <exception cref="OperationCanceledException">Операция отменена.</exception>    
    /// <exception cref="ArgumentNullException">Неверные входные данные.</exception>
    public async Task<EventResponseDto> UpdateEventAsync(Guid id, EventUpdateDTO ev, CancellationToken token)
    {        
        await _eventValidator.ValidateAsync(ev, token);

        token.ThrowIfCancellationRequested();

        try
        { 
            var e = await GetById(id, token);
            e.Title = ev.Title;
            e.Description = ev.Description;
            e.StartAt = ev.StartAt.HasValue ? ev.StartAt.Value : throw new ArgumentNullException("Дата начала события должна быть заполнена");
            e.EndAt = ev.EndAt.HasValue ? ev.EndAt.Value : throw new ArgumentNullException("Дата окончания события должна быть заполнена");
                
            await _eventRepository.SaveChangesAsync(token);
            return e.ToResponse();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Ошибка при сохранении события id = {id}", ex);
        }
    }

    private async Task<Event> GetById(Guid id, CancellationToken token)
    {
        var ev = await _eventRepository.GetEventByIdAsync(id, token);
        if (ev == null)
            throw new NotFoundException($"Не найдено событие с id = {id}");

        return ev;
    }
}
