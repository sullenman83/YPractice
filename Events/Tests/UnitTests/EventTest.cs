using Events.Domain.Models;
using FluentAssertions;
using TestData;



namespace Events.UnitTests
{
    public class EventTest
    {
        [Fact]
        public void TryReserveSeats_ReturnsEventReducedSeatsCount()
        {
            // Arrange
            var totalSeats = 10;
            var seatsCount = 1;
            var ev = EventTestData.GetTestEvent(totalSeats);

            // Act
            var res = ev.TryReserveSeats(seatsCount);

            // Assert
            res.Should().BeTrue();
            ev.AvailableSeats.Should().Be(totalSeats - seatsCount);            
        }

        [Fact]
        public void TryReserveSeats_NegativeCount_ThrowsArgumentException()
        {
            // Arrange
            var totalSeats = 10;
            var seatsCount = -1;
            var ev = EventTestData.GetTestEvent(totalSeats);

            // Act
            Action act = () => ev.TryReserveSeats(seatsCount);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Количество резервируемых мест не может быть отрицательным");
        }

        [Fact]
        public void TryReserveSeats_SeatsCountMoreAvailable_ReturnsFalse()
        {
            // Arrange
            var totalSeats = 10;
            var seatsCount = totalSeats + 1;
            var ev = EventTestData.GetTestEvent(totalSeats);

            // Act
            var res = ev.TryReserveSeats(seatsCount);

            // Assert
            res.Should().BeFalse();
        }

        [Fact]
        public void TryReleaseSeats_ReturnsREstoresSeatCount()
        {
            // Arrange
            var totalSeats = 10;
            var seatsCount = 1;
            var ev = EventTestData.GetTestEvent(totalSeats);

            // Act
            ev.TryReserveSeats(seatsCount);
            var res = ev.ReleaseSeats(seatsCount);

            // Assert
            res.Should().BeTrue();
            ev.AvailableSeats.Should().Be(totalSeats);
        }

        [Fact]
        public void TryReleaseSeats_NegativeSeatsCount_ThrowsArgumentException()
        {
            // Arrange
            var totalSeats = 10;
            var seatsCount = -1;
            var ev = EventTestData.GetTestEvent(totalSeats);

            // Act            
            Action act =() => ev.ReleaseSeats(seatsCount);

            // Assert            
            act.Should().Throw<ArgumentException>().WithMessage("Количество освобождаемых мест не может быть отрицательным");
        }

        [Fact]
        public void TryReleaseSeats_TooManySeats_ReturnsFalse()
        {
            // Arrange
            var totalSeats = 10;
            var seatsCount = 1;
            var ev = EventTestData.GetTestEvent(totalSeats);

            // Act
            ev.TryReserveSeats(seatsCount);
            var res = ev.ReleaseSeats(seatsCount + 1);

            // Assert
            res.Should().BeFalse();
            ev.AvailableSeats.Should().Be(totalSeats - seatsCount);
        }
    }
}
