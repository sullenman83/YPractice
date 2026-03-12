using EventManagement.Interfaces;
using EventManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController(IEventService eventService) : ControllerBase
{

    /// <summary>
    /// Хранилище событий
    /// </summary>
    private readonly IEventService _eventService = eventService;
    [HttpGet]
    public ActionResult<List<EventResponseDto>> GetAllEvents()
    {
        return _eventService.GetAllEvents();
    }
    
    [HttpGet("/{id}")]
    public ActionResult<EventResponseDto> GetEventById(int id)
    {
        return _eventService.GetEventById(id);
    }

    [HttpPost]
    public ActionResult Create([FromBody] EventRequestDto @event)
    {
        _eventService.CreateEvent(@event);

        return Ok();
    }

    [HttpPut("/{id}")]
    public ActionResult Create(int id, [FromBody] EventRequestDto @event)
    {
        _eventService.UpdateEvent(id, @event);

        return Ok();
    }

    [HttpDelete("/{id}")]
    public ActionResult Delete(int id)
    {
        _eventService.DeleteEvent(id);

        return Ok();
    }
}
