using EventManagement.Application.Interfaces.Services.BookingServices;
using EventManagement.Application.Interfaces.Services.EventServices;
using EventManagement.Application.Models.BookingModels;
using EventManagement.Application.Models.Events;
using EventManagement.Application.Models.FilterModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;

namespace EventManagement.Presentation.Controllers;

/// <summary>
/// Контроллер с API для работы с событиями
/// </summary>
/// <param name="eventService">Сервис для работы с событиями</param>
/// <param name="bookingService">Сервис бронирования событий</param>
[Authorize (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
    /// <param name="token">Токен отмены операции</param>
    /// <response code="200">Возвращает HTTP статус-код 200 в случае успешного ответа</response>    
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType<PaginatedResultDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllEventsAsync([FromQuery] EventFilterRequestDTO filter,CancellationToken token)
    {       
        var res = await _eventService.GetEventsAsync(filter, token);
        
        return Ok(res);
    }

    /// <summary>
    /// Получить событие по заданному id
    /// </summary>
    /// <param name="id">Id искомого события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <response code="200">Возвращает HTTP статус-код 200 в случае успешного ответа</response>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType<EventResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEventByIdAsync(Guid id, CancellationToken token)
    {
        var res = await _eventService.GetEventByIdAsync(id, token);
        
        return Ok(res);
    }

    /// <summary>
    /// Создать новое событие
    /// </summary>
    /// <param name="event">Данные события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <response code="201">Возвращает HTTP статус-код 201 в случае успешного ответа</response>    
    [Produces("application/json")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status403Forbidden)]
    [Authorize( Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] EventCreationDTO @event, CancellationToken token)
    {      
        var res = await _eventService.CreateEventAsync(@event, token);

        return CreatedAtAction(nameof(GetEventByIdAsync), new { id = res.Id}, res);
    }

    /// <summary>
    /// Изменить событие с заданным id
    /// </summary>
    /// <param name="id">id события</param>    
    /// <param name="event">Данные события</param>    
    /// <param name="token">Токен отмены операции</param>
    /// <response code="204">Возвращает HTTP статус-код 204 в случае успешного ответа</response>    
    [Produces("application/json")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] EventUpdateDTO @event, CancellationToken token)
    {
        await _eventService.UpdateEventAsync(id, @event, token);
        
        return NoContent();
    }

    /// <summary>
    /// Удалить событие с заданным id
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="token">Токен отмены операции</param>
    /// <response code="200">Возвращает HTTP статус-код 200 в случае успешного ответа</response>        
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken token)
    {
        await _eventService.DeleteEventAsync(id, token);        
        
        return NoContent();
    }

    /// <summary>
    /// Создать новое бронирование
    /// </summary>
    /// <param name="id">Id события</param>
    /// <param name="seatsCount">Количество мест для бронирования</param> 
    /// <param name="token">Токен отмены операции</param>
    /// <response code="202">Возвращает HTTP статус-код 202 в случае успешного ответа</response>
    [Produces("application/json")]
    [ProducesResponseType<BookingResponseDTO>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status500InternalServerError)]
    [HttpPost("{id}/book")]
    public async Task<IActionResult> CreateBooking(Guid id,
        [Required] [Range(1, int.MaxValue)]int seatsCount, 
        CancellationToken token)
    {
        var userId = HttpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userId == null)
            throw new InvalidCredentialException("Пользователь не авторизован");

        var result = await _bookingService.CreateBookingAsync(id, new Guid(userId), seatsCount, token);

        var values = new RouteValueDictionary
        {
            { "controller", "bookings" },
            { "action", "GetBookingByIdAsync" },
            { "id", result.Id }
        };

        return AcceptedAtRoute(values, result);
    }
}
