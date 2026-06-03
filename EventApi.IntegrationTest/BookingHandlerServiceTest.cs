
using EventManagement.Application.Common.AppSettings;
using EventManagement.Application.Interfaces.Reposirories;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Services.BookingServices;
using EventManagement.Common;
using EventManagement.Domain.Interfaces;
using EventManagement.Domain.Models;
using EventManagement.Domain.Services;
using EventManagement.Infrastructure.Services.BookingServices;
using EventManagement.Infrastructure.Services.TransactionService;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventApi.IntegrationTest;

public class BookingHandlerServiceTest : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BookingHandlerServiceTest(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _dateTimeProvider = new DateTimeProvider();
        var serviceCollection = new ServiceCollection();
        serviceCollection.Configure<BookingHandlerSettings>(options => 
        { 
            options.ProcessingDelay = 1000;
            options.PollingInterval = 1000;
        });
        serviceCollection.AddLogging(builder => builder.AddDebug());
        serviceCollection.AddScoped<IDateTimeProvider, DateTimeProvider>();
        serviceCollection.AddScoped<IBookingRepository<Booking>, BookingRepository>();
        serviceCollection.AddScoped<IBackgroundBookingService, BackgroundBookingService>();
        serviceCollection.AddScoped<ITransactionService, TransactionService>();
        serviceCollection.AddScoped(provider =>
        {
            return _fixture.Context;
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();
        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task ConfirmBooking_ChangeStatusToConfirm()
    {
        // Arrange
        var ev = TestData.GetTestEvent();
        await using var context = _fixture.Context;
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        var booking = TestData.GetTestBooking(ev, _dateTimeProvider.GetUtcNow());
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        // Act
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBackgroundBookingService>();
        await service.ConfirmBookingAsync(booking.Id, CancellationToken.None);

        // Assert
        await using var ctx = _fixture.Context;
        var result = await ctx.Bookings.FirstOrDefaultAsync(o => o.Id == booking.Id);
        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task RejectBooking_ChangeStatusToReject()
    {
        // Arrange
        var seatsCount = 2;
        var ev = TestData.GetTestEvent(seatsCount);
        await using var context = _fixture.Context;
        await context.Events.AddAsync(ev);
        await context.SaveChangesAsync();
        var booking = new Booking(BookingStatus.Pending, ev.Id, seatsCount, _dateTimeProvider.GetUtcNow());
        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider.Setup(o => o.GetUtcNow()).Returns(_dateTimeProvider.GetUtcNow().AddMinutes(1));        
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();
        var availableSeats = ev.AvailableSeats;

        // Act
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBackgroundBookingService>();
        await service.RejectBookingAsync(booking.Id, CancellationToken.None);

        // Assert        
        await using var ctx = _fixture.Context;
        var e = await ctx.Events.FirstOrDefaultAsync(o => o.Id == ev.Id);
        var b = await ctx.Bookings.FirstOrDefaultAsync(o => o.Id == booking.Id);
        b.Should().NotBeNull();
        e.Should().NotBeNull();
        e.AvailableSeats.Should().Be(seatsCount);
        b.Status.Should().Be(BookingStatus.Rejected);
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
