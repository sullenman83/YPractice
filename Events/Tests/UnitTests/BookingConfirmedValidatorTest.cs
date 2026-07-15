using DateTimeManager.Abstractions;
using Events.Application.Services.Validators;
using Events.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace Events.UnitTests;

public class BookingConfirmedValidatorTest
{
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider = new Mock<IDateTimeProvider>();
    private readonly DateTimeOffset _date = DateTimeOffset.UtcNow;

    public BookingConfirmedValidatorTest()
    {
        _mockDateTimeProvider.Setup(o => o.GetUtcNow()).Returns(_date);
    }

    [Fact]
    public void ValidateEventDate_CorrectDate_NoThrows()
    {
        // Arrange
        var d = _date.AddDays(1);
        var validator = new BookingConfirmedValidator(_mockDateTimeProvider.Object);

        // Act
        Action act = () => validator.ValidateEventDate(d);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateEventDate_IncorrectDate_ThrowsPastEventBookingException()
    {
        // Arrange
        var d = _date.AddDays(-1);
        var validator = new BookingConfirmedValidator(_mockDateTimeProvider.Object);

        // Act
        Action act = () => validator.ValidateEventDate(d);

        // Assert
        act.Should().Throw<PastEventBookingException>();
    }
}
