using EventManagement.Common.Exceptions;
using EventManagement.Common.Results;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace EventManagement.Controllers;

/// <summary>
/// Контроллер с API для работы с событиями
/// </summary>
/// <param name="eventService">Сервис для работы с событиями</param>
[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{

    /// <summary>
    /// Хранилище событий
    /// </summary>
    private readonly IEventService _eventService = eventService;

    /// <summary>
    /// Метод получения списка событий
    /// </summary>
    /// <param name="filter">Фильтр событий</param>
    /// <response code="200">Возвращает HTTP статус-код 200 в случае успешного ответа</response>    
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status500InternalServerError)]
    public IActionResult GetAllEvents([FromQuery] EventFilterRequestDTO filter)
    {       
        var res = _eventService.GetEvents(filter);

        if (!res.IsSuccess)
            raiseError(res.StatusCode, res.Message);
        
        return Ok(res.Value);
    }

    /// <summary>
    /// Получить событие по заданному id
    /// </summary>
    /// <param name="id">Id искомого события</param>
    /// <response code="200">Возвращает HTTP статус-код 200 в случае успешного ответа</response>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status500InternalServerError)]
    public IActionResult GetEventById(int id)
    {
        var res = _eventService.GetEventById(id);
        if (!res.IsSuccess)
            raiseError(res.StatusCode, res.Message);
        
        return Ok(res.Value);
    }
        
    /// <summary>
    /// Создать новое событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <response code="201">Возвращает HTTP статус-код 201 в случае успешного ответа</response>    
    [Produces("application/json")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status400BadRequest)]
    [HttpPost]
    public IActionResult Create([FromBody] EventRequestDto @event)
    {      
        var res = _eventService.CreateEvent(@event);
        if (!res.IsSuccess)
            raiseError(res.StatusCode, res.Message);
        
        return CreatedAtAction(nameof(GetEventById), new { id = res.Value?.Id}, res.Value);
    }

    /// <summary>
    /// Изменить событие с заданным id
    /// </summary>
    /// /// <param name="id">id события</param>    
    /// <param name="event">Данные события</param>    
    /// <response code="204">Возвращает HTTP статус-код 204 в случае успешного ответа</response>    
    [Produces("application/json")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status500InternalServerError)]
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] EventRequestDto @event)
    {
        var res = _eventService.UpdateEvent(id, @event);

        if (!res.IsSuccess)
            raiseError(res.StatusCode, res.Message);
        
        return Ok(res.Value);
    }


    /// <summary>
    /// Удалить событие с заданным id
    /// </summary>
    /// <param name="id">id события</param>
    /// <response code="200">Возвращает HTTP статус-код 200 в случае успешного ответа</response>    
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status404NotFound)]    
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status500InternalServerError)]
    public IActionResult Delete(int id)
    {
        var res = _eventService.DeleteEvent(id);
        if (!res.IsSuccess)
            raiseError(res.StatusCode, res.Message);
        
        return Ok();
    }

    private void raiseError(ResultStatusCode code, string message)
    {
        switch (code)
        {
            case ResultStatusCode.NotFound: throw new ArgumentException(message); //HttpStatusCode.NotFound
            case ResultStatusCode.ValidationError: throw new ValidationException(message);  //HttpStatusCode.BadRequest
            case ResultStatusCode.InternalError:  throw new InvalidOperationException(message); // HttpStatusCode.InternalServerError

            default: throw new InvalidOperationException(message); // HttpStatusCode.InternalServerError
        }
        ;
    }


    //private int getStatusCode(ResultStatusCode code)
    //{
    //    switch(code)
    //    {
    //        case ResultStatusCode.NotFound: return 404; //HttpStatusCode.NotFound
    //        case ResultStatusCode.ValidationError: return  400;  //HttpStatusCode.BadRequest
    //        case ResultStatusCode.InternalError: return 500; // HttpStatusCode.InternalServerError

    //        default: return 500; // HttpStatusCode.InternalServerError
    //    };
    //}

    //private ObjectResult error<T>(Result<T> result) where T: class
    //{
    //    //Я так понимаю тут возвращаться будет файтически BadRequest. Не знаю имеет смысл пытаться вернуть что-то более типизированное например NotFound?
    //    var status = getStatusCode(result.StatusCode);
    //    return Problem(
    //        result.Message,
    //        "www.hz_kuda.ru",
    //        status,
    //        "Ошибка",
    //        "www.hz_kuda.ru");
    //}
}
