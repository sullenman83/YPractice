using EventManagement.Interfaces;
using System.ComponentModel;

namespace EventManagement.Services;

/// <summary>
/// Фоновый сервис обработки бронирований
/// </summary>
public class BookingHandlerService(ILogger<BackgroundService> logger, IBookingRepository repository ) : BackgroundService
{
    private readonly ILogger<BackgroundService> _logger= logger;
    private readonly IBookingRepository _bookingRepository = repository;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
