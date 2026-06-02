using EventManagement.Application.Interfaces.Reposirories;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Models.BookingModels;
using EventManagement.Application.Services.BookingServices;
using EventManagement.Common;
using EventManagement.Interfaces;
using EventManagement.Models.Events;
using EventManagement.Services;
using FluentAssertions;
using FluentAssertions.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventServiceTest;

public class BackgroundBookingServiceTest
{
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory = new Mock<IServiceScopeFactory>();
    private readonly Mock<IServiceProvider> _mockProvider = new Mock<IServiceProvider>();
    private readonly Mock<IServiceScope> _mockScope = new Mock<IServiceScope>();
    private readonly Mock<IBookingRepository<Booking>> _mockBookingRepository = new Mock<IBookingRepository<Booking>>();
    private readonly Mock<ITransactionService> _mockTransactionService = new Mock<ITransactionService>();
    private readonly Mock<ITransaction> _mockTransaction = new Mock<ITransaction>();
    private readonly NullLogger<BackgroundBookingService> _logger = NullLogger<BackgroundBookingService>.Instance;

    public BackgroundBookingServiceTest()
    {
        _mockTransactionService.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(_mockTransaction.Object);
        _mockProvider.Setup(o => o.GetService(typeof(IDateTimeProvider))).Returns(new DateTimeProvider());
        _mockProvider.Setup(o => o.GetService(typeof(IBookingRepository<Booking>))).Returns(_mockBookingRepository.Object);
        _mockProvider.Setup(o => o.GetService(typeof(ITransactionService))).Returns(_mockTransactionService.Object);
        _mockScope.Setup(o => o.ServiceProvider).Returns(_mockProvider.Object);
        _mockScopeFactory.Setup(o => o.CreateScope()).Returns(_mockScope.Object);
    }

    [Fact]
    public async Task ConfirmBookingAsync_OneSeat_ReturnsReducedSeatsNumber()
    {
        // Arrange
        var seatsCount = 1;
        var ev = TestData.GetTestEvent(seatsCount);
        var id = ev.Id;        
        var booking = new Booking(BookingStatus.Pending, ev, seatsCount, DateTimeOffset.UtcNow);
        _mockBookingRepository.Setup(o => o.GetBookingWithBlockingAsync(It.IsAny<Guid>())).ReturnsAsync(booking);
        var service = new BackgroundBookingService(_mockScopeFactory.Object, _logger);

        // Act
        await service.ConfirmBookingAsync(id, CancellationToken.None);

        // Assert        
        ev.AvailableSeats.Should().Be(0);
        _mockTransactionService.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _mockBookingRepository.Verify(o => o.GetBookingWithBlockingAsync(It.IsAny<Guid>()), Times.Once);

        _mockBookingRepository.Verify(o => o.SaveChangesAsync(), Times.Once);
        _mockTransaction.Verify(o => o.CommitAsync(), Times.Once);
        _mockTransaction.Verify(o => o.RollbackAsync(), Times.Never);
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task ConfirmBookingAsync_SeatsCountMoreThenAvailable_ReturnsBookingWithRejectStatus()
    {
        // Arrange
        var seatsCount = 1;
        var ev = TestData.GetTestEvent(seatsCount);
        var id = ev.Id;
        var booking = new Booking(BookingStatus.Pending, ev, seatsCount + 1, DateTimeOffset.UtcNow);
        _mockBookingRepository.Setup(o => o.GetBookingWithBlockingAsync(It.IsAny<Guid>())).ReturnsAsync(booking);
        var service = new BackgroundBookingService(_mockScopeFactory.Object, _logger);

        // Act
        await service.ConfirmBookingAsync(id, CancellationToken.None);

        // Assert        
        ev.AvailableSeats.Should().Be(1);
        _mockTransactionService.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _mockBookingRepository.Verify(o => o.GetBookingWithBlockingAsync(It.IsAny<Guid>()), Times.Once);

        _mockBookingRepository.Verify(o => o.SaveChangesAsync(), Times.Once);
        _mockTransaction.Verify(o => o.CommitAsync(), Times.Once);
        _mockTransaction.Verify(o => o.RollbackAsync(), Times.Never);
        booking.Status.Should().Be(BookingStatus.Rejected);
    }

    [Fact]
    public async Task RejectTest_ReturnsBookingWithRejectStatus()
    {
        // Arrange
        var seatsCount = 1;
        var ev = TestData.GetTestEvent(seatsCount);
        var id = ev.Id;
        var booking = new Booking(BookingStatus.Pending, ev, seatsCount, DateTimeOffset.UtcNow);
        _mockBookingRepository.Setup(o => o.GetBookingWithBlockingAsync(It.IsAny<Guid>())).ReturnsAsync(booking);
        var service = new BackgroundBookingService(_mockScopeFactory.Object, _logger);

        // Act
        await service.RejectBookingAsync(id, CancellationToken.None);

        // Assert        
        ev.AvailableSeats.Should().Be(1);
        _mockTransactionService.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _mockBookingRepository.Verify(o => o.GetBookingWithBlockingAsync(It.IsAny<Guid>()), Times.Once);
        _mockBookingRepository.Verify(o => o.SaveChangesAsync(), Times.Once);
        _mockTransaction.Verify(o => o.CommitAsync(), Times.Once);
        _mockTransaction.Verify(o => o.RollbackAsync(), Times.Never);
        booking.Status.Should().Be(BookingStatus.Rejected);
    }
}
