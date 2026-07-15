using Bookings.Application.AppSettings;
using Bookings.Application.Interfaces.BookingServices;
using Bookings.Application.Interfaces.Repositories;
using DateTimeManager.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Bookings.Application.Services;

/// <summary>
/// Фоновый сервис обработки бронирований
/// </summary>
public class BackgroundBookingService(ILogger<BackgroundService> logger, 
    IServiceScopeFactory serviceFactory, 
    IOptions<BackgroundBookingServiceSettings> bookingHandlerSettings) : BackgroundService
{
    private readonly ILogger<BackgroundService> _logger= logger;
    private readonly IServiceScopeFactory _serviceFactory = serviceFactory;
    private readonly BackgroundBookingServiceSettings _bookingHandlerSettings = bookingHandlerSettings.Value;

    /// <summary>
    /// Метод сервиса фоновой обработки броней
    /// </summary>
    /// <param name="stoppingToken">Токен отмены</param>
    /// <returns>Пустая задача</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис обработки бронирований запущен.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _serviceFactory.CreateAsyncScope();                
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var ids = (await bookingRepository.GetPendingBookingsAsync(stoppingToken))
                    .Select(o => o.Id)
                    .ToList();               
                
                var tasks = ids.Select(o => ProcessBookingAsync(o, stoppingToken));

                await Task.WhenAll(tasks);
                
                await Task.Delay(TimeSpan.FromMilliseconds(_bookingHandlerSettings.PollingInterval), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при бронировании события.");            
            }
        }

        _logger.LogInformation("Фоновый сервис обработки бронирований остановлен.");
    }

    private async Task ProcessBookingAsync(Guid id, CancellationToken stoppingToken)
    {        
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(_bookingHandlerSettings.ProcessingDelay), stoppingToken);

            await using var scope =  _serviceFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IBookingHandlerService>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
            await service.ConfirmBookingAsync(id, stoppingToken);

            _logger.LogInformation($"Бронирование с id {id} обработано в {dateTimeProvider.GetUtcNow()}.");
        }        
        catch
        {
            await using var scope = _serviceFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IBookingHandlerService>();
            await service.RejectBookingAsync(id, stoppingToken);
                
            throw;
        }        
    }
}