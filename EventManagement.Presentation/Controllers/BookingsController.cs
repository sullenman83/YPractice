using EventManagement.Application.Interfaces.Services.BookingServices;
using EventManagement.Application.Models.BookingModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;

namespace EventManagement.Presentation.Controllers;


/// <summary>
/// Контроллер для бронирования событий
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookingByIdAsync(Guid id, CancellationToken token)
    {
        var result = await _bookingService.GetBookingByIdAsync(id, token);
        
        return Ok(result);
    }

    /// <summary>
    /// Отменить бронирование
    /// </summary>
    /// <param name="id">id брони</param>
    /// <param name="token">Токен отмены</param>
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelBooking(Guid id, CancellationToken token)
    {
        var userId = HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userId == null)
            throw new InvalidCredentialException("Пользователь не авторизован");

        await _bookingService.CancelBookingAsync(id, new Guid(userId), token);

        return NoContent();
    }
}
