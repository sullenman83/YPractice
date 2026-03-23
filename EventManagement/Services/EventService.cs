using EventManagement.Common;
using EventManagement.Common.Exceptions;
using EventManagement.Common.Results;
using EventManagement.Extensions;
using EventManagement.Interfaces;
using EventManagement.Models;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using System.Collections.Concurrent;

namespace EventManagement.Services;

/// <summary>
/// Сервис для работы с событиями
/// </summary>
public class EventService(IEventValidator eventValidator, IEventRepository repository) : IEventService
{
    /// <summary>
    /// Костыль для получения id вставляемой записи. Не придумал лучшего способа получить MAX id из словаря.
    /// Когда перйдем на БД все это уберется
    /// </summary>
    private static readonly Lock _lock = new Lock();    

    private readonly IEventValidator _eventValidator = eventValidator;
    private readonly IEventRepository _repository = repository;

    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <returns>Обновленное событие</returns>
    /// <exception cref="InvalidOperationException">Ошибка при создании нового события.</exception>
    public Result<EventResponseDto> CreateEvent(EventRequestDto @event)
    {
        try
        {
            _eventValidator.Validate(@event);
            //Блокирую словарь. Получаю максимальный id. Создаю событие и вставляю его используя непотокобезопасный метод add так как солловарь все равно уже залочен
            //знаю костыль, но как иначе не придумал. А много времени думать у меня к сожалению нет. Если придумается на подсознательном уровне переделаю пока так
            using (_lock.EnterScope())
            {                            
                var ev = createEvent(@event);
                (_repository.Data as IDictionary<int, Event>).Add(ev.Id, ev);

                var res = createEventResponseDto(ev);
                return createResult<EventResponseDto>(res);            
            }
        }
        catch (Exception ex)
        {
            return createErrorRsulte<EventResponseDto>(ex);
        }
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">Идентификатор удаляемого события</param>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    public Result<EventResponseDto> DeleteEvent(int id)
    {
        try
        {
            if (!_repository.Data.TryRemove(id, out var ev))
                throw new ArgumentException($"Ошбика при удалении события {id}.");

            return createResult<EventResponseDto>();            
        }
        catch (Exception ex)
        {
            return createErrorRsulte<EventResponseDto>(ex);            
        }
    }

    /// <summary>
    /// Получить все события
    /// </summary>
    /// <param name="filter">Фильтр событий</param>
    /// <returns>Список событий</returns>
    public Result<PaginatedResultDTO> GetEvents(EventFilterRequestDTO filter)
    {
        try
        {
            var events = _repository.Data.ToArray()
                .Select(o => o.Value)
                .OrderBy(o => o.StartAt)
                .Filter(filter)
                .Paginate(filter)
                .Select(o => createEventResponseDto(o))
                .ToList();

            var res = new PaginatedResultDTO()
            {
                Events = events,
                EventsCount = _repository.Data.Count,
                Page = filter.Page,
                EventsCountOnCurrentPage = events.Count
            };


            return createResult<PaginatedResultDTO>(res);             
        }
        catch (Exception ex)
        {
            return createErrorRsulte<PaginatedResultDTO>(ex);            
        }
    }

    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <returns>Событие с искомым идентификатором</returns>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    public Result<EventResponseDto> GetEventById(int id)
    {
        try
        {
            if (!_repository.Data.TryGetValue(id, out var ev))
                throw new ArgumentException($"Ошбика при получении события по {id}.");

            var res = createEventResponseDto(ev);

            return createResult<EventResponseDto>(res);
        }
        catch (Exception ex)
        {
            return createErrorRsulte<EventResponseDto>(ex);   
        }
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="event">Данные события</param>
    /// /// <returns>Обновленное событие</returns>
    /// <exception cref="ArgumentException">Не найдено событие с заданным id</exception>
    public Result<EventResponseDto> UpdateEvent(int id, EventRequestDto @event)
    {
        try
        {
            _eventValidator.Validate(@event);
            var ev = getEventById(id);
            var newEv = ev.Clone() as Event;

            updateEvent(@event, newEv);
            if (!_repository.Data.TryUpdate(id, newEv, ev))
                throw new ArgumentException($"Ошбика при обновлении события по {id}.");

            var res = createEventResponseDto(newEv);

            return createResult<EventResponseDto>(res);
        }
        catch (Exception ex)
        {
            return createErrorRsulte<EventResponseDto>(ex);
        }
    }


    private Event createEvent(EventRequestDto source)
    {
        var id = _repository.Data.Keys.Max() + 1;

        return createEvent(id, source);        
    }

    private Event createEvent(int id, EventRequestDto source)
    {
        return new Event()
        {
            Id = id,
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

    private Event getEventById(int id)
    {
        if (!_repository.Data.TryGetValue(id, out var ev))
            throw new ArgumentException($"Не найдено событие с id = {id}");
        
        return ev!;
    }

    private ResultStatusCode getStatusCode(Exception ex)
    {
        return ex switch
        {
            ArgumentException e => ResultStatusCode.NotFound,
            EventValidationException e => ResultStatusCode.ValidationError,
            _ => ResultStatusCode.InternalError
        };
    }

    private Result<T> createErrorRsulte<T>(Exception ex) where T : class
    {
        return new Result<T>()
        {
            IsSuccess = false,
            StatusCode = getStatusCode(ex),
            Message = ex.Message
        };
    }

    private Result<T> createResult<T>(T? value = null, ResultStatusCode code = ResultStatusCode.Ok, string message = "", bool isSuccess = true) where T: class
    {
        return new Result<T>()
        {
            IsSuccess = isSuccess,
            Value = value,
            StatusCode = code,
            Message = message
        };
    }
}
