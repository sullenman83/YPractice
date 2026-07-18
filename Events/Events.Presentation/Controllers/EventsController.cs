using Events.Application.Interfaces;
using Events.Application.Models;
using Events.Application.Models.FilterModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Presentation.Controllers;

/// <summary>
/// Контроллер с API для работы с событиями
/// </summary>
/// <param name="eventService">Сервис для работы с событиями</param>
[Authorize (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
    /// <param name="token">Токен отмены операции</param>
    /// <response code="200">Возвращает HTTP статус-код 200 в случае успешного ответа</response>    
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType<List<PaginatedResultDTO>>(StatusCodes.Status200OK)]
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
    /// Получить топ 10 событий по популярности
    /// </summary>    
    /// <param name="token">Токен отмены операции</param>
    /// <response code="200">Возвращает HTTP статус-код 200 в случае успешного ответа</response>
    [HttpGet("top")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType<List<EventResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTop10Events(CancellationToken token)
    {
        var res = await _eventService.GetTop10Events(token);

        return Ok(res);
    }
}
