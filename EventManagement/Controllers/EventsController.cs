using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using EventManagement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

/// <summary>
/// Контроллер с API для работы с событиями
/// </summary>
/// <param name="eventService">Сервис для работы с событиями</param>
/// <param name="bookingService">Сервис бронирования событий</param>
[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService, IBookingService bookingService) : ControllerBase
{
    /// <summary>
    /// Хранилище событий
    /// </summary>
    private readonly IEventService _eventService = eventService;
    private readonly IBookingService _bookingService = bookingService;

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
        
        return Ok(res);
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
    public IActionResult GetEventById(Guid id)
    {
        var res = _eventService.GetEventById(id);
        
        return Ok(res);
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

        return CreatedAtAction(nameof(GetEventById), new { id = res.Id}, res);
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
    public IActionResult Update(Guid id, [FromBody] EventRequestDto @event)
    {
        var res = _eventService.UpdateEvent(id, @event);
        
        return Ok(res);
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
    public IActionResult Delete(Guid id)
    {
        _eventService.DeleteEvent(id);        
        
        return Ok();
    }


    /// <summary>
    /// Создать новое бронирование
    /// </summary>
    /// <param name="id">Id события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <response code="202">Возвращает HTTP статус-код 202 в случае успешного ответа</response>
    [Produces("application/json")]
    [ProducesResponseType<BookingResponseDTO>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("{id}/book")]
    public async Task<IActionResult> CreateBooking(Guid id, CancellationToken token)
    {
        var result = await _bookingService.CreateBookingAsync(id, token);

        var values = new RouteValueDictionary
        {
            { "controller", "bookings" },
            { "action", "GetBookingByIdAsync" },
            { "id", result.Id }
        };

        return AcceptedAtRoute(values, result);
    }
}
