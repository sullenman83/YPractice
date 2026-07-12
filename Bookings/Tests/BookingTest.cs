using Bookings.Domain.Models;
using DateTimeManager.Abstractions;
using FluentAssertions;
using Moq;

namespace UnitTest;

public class BookingTest
{
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider = new Mock<IDateTimeProvider>();
    private readonly DateTimeOffset _date;

    public BookingTest()
    {
        _date = DateTimeOffset.UtcNow;
        _mockDateTimeProvider.Setup(o => o.GetUtcNow()).Returns(_date);
    }
    
    [Fact]
    public void BookingRejectTest_BookingHasRejectedStatus()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var status = BookingStatus.Pending;
        var seatsCount = 1;

        var booking = new Booking(status , eventId, userId, seatsCount, _mockDateTimeProvider.Object.GetUtcNow());

        // Act
        booking.Reject(_mockDateTimeProvider.Object.GetUtcNow());

        //Assert
        booking.Status.Should().Be(BookingStatus.Rejected);
        booking.ProcessedAt.Should().Be(_date);
    }

    [Fact]
    public void BookingConfirmTest_BookingHasConfirmeddStatus()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var status = BookingStatus.Pending;
        var seatsCount = 1;

        var booking = new Booking(status, eventId, userId, seatsCount, _mockDateTimeProvider.Object.GetUtcNow());

        // Act
        booking.Confirm(_mockDateTimeProvider.Object.GetUtcNow());

        //Assert
        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.ProcessedAt.Should().Be(_date);
    }

    [Fact]
    public void BookingCancelTest_BookingHasCancelleddStatus()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var status = BookingStatus.Pending;
        var seatsCount = 1;

        var booking = new Booking(status, eventId, userId, seatsCount, _mockDateTimeProvider.Object.GetUtcNow());

        // Act
        booking.Cancel(_mockDateTimeProvider.Object.GetUtcNow());

        //Assert
        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.ProcessedAt.Should().Be(_date);
    }
}
