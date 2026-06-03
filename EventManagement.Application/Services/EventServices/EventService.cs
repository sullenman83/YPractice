using EventManagement.Application.Common.Exceptions;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Reposirories;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Models.Events;
using EventManagement.Application.Models.Events.Extensions;
using EventManagement.Application.Models.FilterModels;
using EventManagement.Domain.Models;
namespace EventManagement.Application.Services.EventServices;

/// <summary>
/// Сервис для работы с событиями
/// </summary>
public class EventService(IEventValidator eventValidator, IEventRepository<Event> eventRepository) : IEventService
{
    private readonly IEventValidator _eventValidator = eventValidator;
    private readonly IEventRepository<Event> _eventRepository = eventRepository;

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
    /// <exception cref="DbOperationException">Ошибка операций с БД.</exception>
    /// <exception cref="EventValidationException">Ошибка валидации</exception>    
    /// <exception cref="OperationCanceledException">Операция отменена.</exception>    
    /// <exception cref="ArgumentNullException">Неверные входные данные.</exception>
    public async Task<EventResponseDto> UpdateEventAsync(Guid id, EventUpdateDTO ev, CancellationToken token)
    {        
        _eventValidator.Validate(ev);

        token.ThrowIfCancellationRequested();
         
        var e = await GetById(id, token);
        e.Title = ev.Title;
        e.Description = ev.Description;
        e.StartAt = ev.StartAt.HasValue ? ev.StartAt.Value : throw new ArgumentNullException("Дата начала события должна быть заполнена");
        e.EndAt = ev.EndAt.HasValue ? ev.EndAt.Value : throw new ArgumentNullException("Дата окончания события должна быть заполнена");
                
        await _eventRepository.SaveChangesAsync(token);
        return e.ToResponse();
    }

    private async Task<Event> GetById(Guid id, CancellationToken token)
    {
        var ev = await _eventRepository.GetByIdAsync(id, token);
        if (ev == null)
            throw new NotFoundException($"Не найдено событие с id = {id}");

        return ev;
    }   
}
