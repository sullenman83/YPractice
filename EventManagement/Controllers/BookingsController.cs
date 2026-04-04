using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;


/// <summary>
/// Контроллер для бронирования событий
/// </summary>
[Controller]
[Route("[controller]")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    private readonly IBookingService _bookingService = bookingService;

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

        return Accepted($"/bookings/{result.Id}", result);
    }

    /// <summary>
    /// Получить бронирование по Id
    /// </summary>
    /// <param name="id">Id бпрнирования</param>
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
