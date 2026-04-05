using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;


/// <summary>
/// Контроллер для бронирования событий
/// </summary>
[ApiController]
[Route("[controller]")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    private readonly IBookingService _bookingService = bookingService;
    
    /// <summary>
    /// Получить бронирование по Id
    /// </summary>
    /// <param name="id">Id бронирования</param>
    /// <param name="token">Токен отмены операции</param>
    /// <response code="200">Возвращает HTTP статус-код 200 в случае успешного ответа</response>
    [Produces("application/json")]
    [ProducesResponseType<BookingResponseDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookingByIdAsync(Guid id, CancellationToken token)
    {
        var result =await _bookingService.GetBookingByIdAsync(id, token);
        
        return Ok(result);
    }
    
}
