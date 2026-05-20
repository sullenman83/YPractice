using EventManagement.Common;
using EventManagement.Common.AppSettings;
using EventManagement.Common.Exceptions;
using EventManagement.Data;
using EventManagement.Interfaces;
using EventManagement.Interfaces.Reposirories;
using EventManagement.Interfaces.Services;
using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventManagement.Services;

/// <summary>
/// Фоновый сервис обработки бронирований
/// </summary>
public class BookingHandlerService(ILogger<BackgroundService> logger, IServiceScopeFactory serviceFactory, IOptions<BookingHandlerSettings> bookingHandlerSettings) : BackgroundService
{
    private readonly ILogger<BackgroundService> _logger= logger;
    private readonly IServiceScopeFactory _serviceFactory = serviceFactory;
    private readonly BookingHandlerSettings _bookingHandlerSettings = bookingHandlerSettings.Value;

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
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository<Booking>>();
                var pendingList = (await bookingRepository.GetPendingBookingsAsync(stoppingToken))
                    .Select(o => o.Id);
                var processingList = (await bookingRepository.GetProcessingBookingAsync(stoppingToken))
                    .Select(o => o.Id);

                var ids = pendingList.Concat(processingList)
                    .ToList();

                var tasks = ids.Select(o => ProcessBookingAsync(o, stoppingToken));

                await Task.WhenAll(tasks);
                
                await Task.Delay(TimeSpan.FromSeconds(_bookingHandlerSettings.PollingInterval), stoppingToken);
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
        await Task.Delay(TimeSpan.FromSeconds(_bookingHandlerSettings.ProcessingDelay), stoppingToken);
        try
        {
            await using var scope =  _serviceFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IBackgroundBookingService>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
            await service.ConfirmBookingAsync(id, stoppingToken);

            _logger.LogInformation($"Бронирование с id {id} обработано в {dateTimeProvider.UtcNow}.");
        }
        catch
        {
            await using var scope = _serviceFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IBackgroundBookingService>();
            await service.RejectBookingAsync(id, stoppingToken);
                
            throw;
        }        
    }
}