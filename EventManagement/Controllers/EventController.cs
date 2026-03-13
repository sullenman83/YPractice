using EventManagement.Common.Exceptions;
using EventManagement.Common.Results;
using EventManagement.Interfaces;
using EventManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventManagement.Controllers;

/// <summary>
/// Контроллер с API для работы с событиями
/// </summary>
/// <param name="eventService">Сервис для работы с событиями</param>
[ApiController]
[Route("api/[controller]")]
public class EventController(IEventService eventService) : ControllerBase
{

    /// <summary>
    /// Хранилище событий
    /// </summary>
    private readonly IEventService _eventService = eventService;

    /// <summary>
    /// Метод получения списка событий
    /// </summary>
    /// <response code="200">Возвращает HTTP статус-код 200 в случае успешного ответа</response>    
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(EventApiResult<List<EventResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EventApiResult<List<EventResponseDto>>), StatusCodes.Status500InternalServerError)]
    public EventApiResult<List<EventResponseDto>> GetAllEvents()
    {
        try
        {
            var res = _eventService.GetAllEvents();
            
            return new EventApiResult<List<EventResponseDto>>()
            {
                Data = res,
                DateTime = DateTime.Now,
                Message = "Получили полный список событий",
                Success = true,
                StatusCode = HttpStatusCode.OK
            };
        }
        catch (Exception ex)
        {
            return new EventApiResult<List<EventResponseDto>>()
            {
                Data = null,
                DateTime = DateTime.Now,
                Message = ex.Message,
                Success = false,
                StatusCode = getStatusCode(ex)
            };            
        }
    }

    /// <summary>
    /// Получить событие по заданному id
    /// </summary>
    /// <param name="id">Id искомого события</param>
    /// <response code="200">Возвращает HTTP статус-код 200 в случае успешного ответа</response>
    [HttpGet("/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(EventApiResult<EventResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EventApiResult<EventResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EventApiResult<EventResponseDto>), StatusCodes.Status500InternalServerError)]
    public EventApiResult<EventResponseDto> GetEventById(int id)
    {
        try
        {
            var res = _eventService.GetEventById(id);
            return new EventApiResult<EventResponseDto>()
            {
                Data = res,
                Success = true,
                StatusCode = HttpStatusCode.OK,
                DateTime = DateTime.Now,
                Message = $"Получили событие по id = {id}"
            };

        }
        catch (Exception ex)
        {
            return new EventApiResult<EventResponseDto>()
            {
                Data = null,
                DateTime = DateTime.Now,
                Message = ex.Message,
                Success = false,
                StatusCode = getStatusCode(ex)
            };
        }
    }
        
    /// <summary>
    /// Создать новое событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <response code="201">Возвращает HTTP статус-код 201 в случае успешного ответа</response>    
    [Produces("application/json")]
    [ProducesResponseType(typeof(EventApiResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EventApiResult), StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public EventApiResult Create([FromBody] EventRequestDto @event)
    {
        try
        {
            _eventService.CreateEvent(@event);

            return new EventApiResult()
            {
                Success = true,
                StatusCode = HttpStatusCode.Created,
                DateTime = DateTime.Now,
                Message = "Создали новые событие"
            };
        }
        catch (Exception ex)
        {
            return new EventApiResult
            {                
                DateTime = DateTime.Now,
                Message = ex.Message,
                Success = false,
                StatusCode = getStatusCode(ex)
            };
        }
    }

    /// <summary>
    /// Изменить событие сзаданным id
    /// </summary>
    /// /// <param name="id">id события</param>    
    /// <param name="event">Данные события</param>    
    /// <response code="204">Возвращает HTTP статус-код 204 в случае успешного ответа</response>    
    [Produces("application/json")]
    [ProducesResponseType(typeof(EventApiResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EventApiResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EventApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EventApiResult), StatusCodes.Status500InternalServerError)]
    [HttpPut("/{id}")]
    public EventApiResult Update(int id, [FromBody] EventRequestDto @event)
    {
        try
        {
            _eventService.UpdateEvent(id, @event);
            
            return new EventApiResult()
            {
                Success = true,
                StatusCode = HttpStatusCode.NoContent,
                DateTime = DateTime.Now,
                Message = $"Изменили событие с id = {id}"
            };
        }
        catch (Exception ex)
        {
            return new EventApiResult
            {
                DateTime = DateTime.Now,
                Message = ex.Message,
                Success = false,
                StatusCode = getStatusCode(ex)
            };
        }
    }


    /// <summary>
    /// Удалить событие с заданным id
    /// </summary>
    /// <param name="id">id события</param>
    /// <response code="200">Возвращает HTTP статус-код 200 в случае успешного ответа</response>    
    [HttpDelete("/{id}")]
    [ProducesResponseType(typeof(EventApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EventApiResult), StatusCodes.Status404NotFound)]    
    [ProducesResponseType(typeof(EventApiResult), StatusCodes.Status500InternalServerError)]
    public EventApiResult Delete(int id)
    {
        try
        {
            _eventService.DeleteEvent(id);

            return new EventApiResult()
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                DateTime = DateTime.Now,
                Message = $"Удалили событие с id = {id}"
            };
        }
        catch (Exception ex)
        {
            return new EventApiResult
            {
                DateTime = DateTime.Now,
                Message = ex.Message,
                Success = false,
                StatusCode = getStatusCode(ex)
            };
        }
    }

    private HttpStatusCode getStatusCode(Exception ex)
    {
        return ex switch
        {
            ArgumentException e => HttpStatusCode.NotFound,
            EventValidationException e => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };
    }
}
