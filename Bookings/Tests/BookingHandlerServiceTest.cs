
using Bookings.Application.Interfaces;
using Bookings.Application.Interfaces.BookingServices;
using Bookings.Application.Interfaces.Repositories;
using Bookings.Application.Services.BookingServices;
using Bookings.Domain.Models;
using DateTimeManager.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TransactionManager.Abstractions;

namespace UnitTest;

public class BookingHandlerServiceTest
{
    private readonly Mock<IBookingRepository> _mockBookingRepository = new Mock<IBookingRepository>();
    private readonly Mock<IOutboxMessageRepository> _mockOutboxRepository = new Mock<IOutboxMessageRepository>();
    private readonly Mock<ITransactionService> _mockTransactionService = new Mock<ITransactionService>();
    private readonly Mock<ITransaction> _mockTransaction = new Mock<ITransaction>();
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider = new Mock<IDateTimeProvider>();
    private readonly Mock<ICurrentUserService> _mockCurrentUserService = new Mock<ICurrentUserService>();
    private readonly Mock<IBookingValidator> _mockBookingValidator = new Mock<IBookingValidator>();
    private readonly NullLogger<BookingHandlerService> _logger = NullLogger<BookingHandlerService>.Instance;

    public BookingHandlerServiceTest()
    {
        _mockDateTimeProvider.Setup(o => o.GetUtcNow()).Returns(DateTimeOffset.UtcNow.Date);
        _mockTransactionService.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(_mockTransaction.Object);
        _mockBookingValidator.Setup(o => o.ValidateActiveBooking(It.IsAny<IReadOnlyCollection<Booking>>()));
        _mockCurrentUserService.Setup(o => o.IsInRole(It.IsAny<string>())).Returns(true);        
    }

    [Fact]
    public async Task ConfirmBooking_ReturnsBookingWithConfirmedStatus()
    {
        // Arrange
        var seatsCount = 1;
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventId, userId, seatsCount, DateTimeOffset.UtcNow);
        _mockBookingRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(booking);
        var service = new BookingHandlerService(_logger, _mockOutboxRepository.Object, _mockTransactionService.Object, 
            _mockDateTimeProvider.Object, _mockBookingRepository.Object);

        // Act
        await service.ConfirmBookingAsync(booking.Id, CancellationToken.None);

        // Assert
        _mockTransactionService.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _mockBookingRepository.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        _mockBookingRepository.Verify(o => o.SaveChangesAsync(), Times.Once);
        _mockTransaction.Verify(o => o.CommitAsync(), Times.Once);
        _mockTransaction.Verify(o => o.RollbackAsync(), Times.Never);
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task RejectTest_ReturnsBookingWithRejectStatus()
    {
        // Arrange
        // Arrange
        var seatsCount = 1;
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventId, userId, seatsCount, DateTimeOffset.UtcNow);
        _mockBookingRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(booking);
        var service = new BookingHandlerService(_logger, _mockOutboxRepository.Object, _mockTransactionService.Object,
            _mockDateTimeProvider.Object, _mockBookingRepository.Object);

        // Act
        await service.RejectBookingAsync(booking.Id, CancellationToken.None);

        // Assert
        
        _mockBookingRepository.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        _mockBookingRepository.Verify(o => o.SaveChangesAsync(), Times.Once);        
        booking.Status.Should().Be(BookingStatus.Rejected);
    }
}