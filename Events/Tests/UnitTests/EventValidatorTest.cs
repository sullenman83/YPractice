

using Events.Application.Models;
using Events.Application.Services.Validators;
using Events.Domain.Exceptions;
using FluentAssertions;
using TestData;

namespace Events.UnitTests;

public class EventValidatorTest
{
    [Fact]
    public void Validate_CreationDTO_ValidDate_CorrectDate()
    {
        // Arrange
        var ev = EventTestData.GetTestEventCreationDTO();
        var validator = new EventValidator();

        // Act
        Action act = () =>validator.Validate(ev);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_CreationDTO_InvalidDate_CorrectDate()
    {
        // Arrange
        var ev = EventTestData.GetTestEventCreationDTO();
        ev.StartAt = ev.EndAt!.Value.AddDays(1);
        var validator = new EventValidator();

        // Act
        Action act = () => validator.Validate(ev);

        // Assert
        act.Should().Throw<EventValidationException>().WithMessage("Событие содержит некорректные данные. Дата окончания меньше даты начала.");
    }

    [Fact]
    public void Validate_EventUpdateDTO_ValidDate_CorrectDate()
    {
        // Arrange
        var ev = EventTestData.GetTestEvent();
        var up = new EventUpdateDTO()
        {
            Title = ev.Title + "update",
            Description = ev.Description + "update",
            StartAt = ev.StartAt.AddDays(1),
            EndAt = ev.EndAt.AddDays(1)
        };
        var validator = new EventValidator();

        // Act
        Action act = () => validator.Validate(up);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_EventUpdateDTO_InvalidDate_CorrectDate()
    {
        // Arrange
        var ev = EventTestData.GetTestEvent();
        var up = new EventUpdateDTO()
        {
            Title = ev.Title + "update",
            Description = ev.Description + "update",
            StartAt = ev.EndAt.AddDays(4),
            EndAt = ev.EndAt.AddDays(1)
        };
        var validator = new EventValidator();

        // Act
        Action act = () => validator.Validate(up);

        // Assert
        act.Should().Throw<EventValidationException>().WithMessage("Событие содержит некорректные данные. Дата окончания меньше даты начала.");
    }
}
