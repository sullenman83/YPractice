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
        try
        {
            return _eventService.GetAllEvents();
        }
        catch
        {
            return NotFound();
        }
    }
    
    [HttpGet("/{id}")]
    public ActionResult<EventResponseDto> GetEventById(int id)
    {
        try
        {
            return _eventService.GetEventById(id);
        }
        catch(ArgumentException ex)
        {
            return NotFound();
        }
        catch
        {
            return BadRequest();
        }
        
    }

    [HttpPost]
    public ActionResult Create([FromBody] EventRequestDto @event)
    {
        try
        {
            _eventService.CreateEvent(@event);

            return Ok();
        }
        catch
        {
            return StatusCode(500); 
        }
    }

    [HttpPut("/{id}")]
    public ActionResult Update(int id, [FromBody] EventRequestDto @event)
    {
        try
        {
            _eventService.UpdateEvent(id, @event);
            return Ok();
        }
        catch(ArgumentException ex)
        {
            return NotFound();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpDelete("/{id}")]
    public ActionResult Delete(int id)
    {
        try
        {
            _eventService.DeleteEvent(id);

            return Ok();
        }
        catch
        {
            return StatusCode(500);
        }
    }
}
