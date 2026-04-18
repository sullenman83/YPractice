using EventManagement.Common;
using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;
using EventManagement.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Concurrent;

namespace EventServiceTest;

public class BookingTest
{
    private readonly Mock<IBookingRepository> _bookingRepository;
    private readonly Mock<IEventRepository> _eventRepository;

    public BookingTest()
    {
        _bookingRepository = new Mock<IBookingRepository>();
        _eventRepository = new Mock<IEventRepository>();
    }

    [Fact]
    public async Task CreateBooking_ByEventId_ReturnBookingWithPendingStatus()
    {
        // Arrange        
        var ev = TestData.GetTestEvent();
        var id = ev.Id;
        var seats = 5;
        
        _eventRepository.Setup(o => o.GetByID(id)).Returns(ev);
        _eventRepository.Setup(o => o.Update(ev));
        _bookingRepository.Setup(o => o.Add(It.IsAny<Booking>())).Returns<Booking>(b => b);
        
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        

        // Act
        var result = await service.CreateBookingAsync(id, seats, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(id);
        result.Status.Should().Be(BookingStatus.Pending);        
        ev.AvailableSeats.Should().Be(ev.TotalSeats - seats);
        _eventRepository.Verify(o => o.GetByID(id), Times.Once);
        _eventRepository.Verify(o => o.Update(ev), Times.Once);
        _bookingRepository.Verify(o => o.Add(It.IsAny<Booking>()), Times.Once);
    }

    [Fact]
    public async Task CreateSeveralBookings_ByEventID_ReturnsUniqueBookingId()
    {
        // Arrange        
        var ev = TestData.GetTestEvent();
        var id = ev.Id;
        var ids = new List<Guid>();

        _eventRepository.Setup(o => o.GetByID(id)).Returns(ev);
        _eventRepository.Setup(o => o.Update(ev));
        _bookingRepository.Setup(o => o.Add(It.IsAny<Booking>())).Returns<Booking>(b => b);

        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);
        
        // Act
        for (int i = 0; i < 3; ++i)
        {
            var result = await service.CreateBookingAsync(id, 1, CancellationToken.None);
            ids.Add(result.Id);
        }

        // Assert
        ids.Should().HaveCount(3);
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GetBooking_ById_ReturnBookig()
    {
        // Arrange
        var eventID = Guid.NewGuid();
        var booking =  new Booking(BookingStatus.Pending, eventID, DateTime.Now);
        var id = booking.Id;
        _bookingRepository.Setup(o => o.GetById(id)).Returns(booking);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);
        
        // Act
        var result = await service.GetBookingByIdAsync(booking.Id, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(booking.EventId);
        result.Id.Should().Be(booking.Id);
        result.Status.Should().Be(booking.Status);
    }

    [Fact]
    public async Task GetBooking_ChangeStatus_ReturnChangedStatus()
    {
        // Arrange        
        var eventID = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventID, DateTime.Now);
        var id = booking.Id;
        _bookingRepository.Setup(o => o.GetById(id)).Returns(booking);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);


        // Act
        var result = await service.GetBookingByIdAsync(id, CancellationToken.None);
        booking.Reject();
        var result1 = await service.GetBookingByIdAsync(id, CancellationToken.None);

        // Assert
        result1.Status.Should().NotBe(BookingStatus.Pending);
    }

    [Fact]
    public async Task GetBooking_ByInvalidEventId_SholdThrowsArgumentException()
    {
        // Arrange        
        var eventId = Guid.NewGuid();
        _eventRepository.Setup(o => o.GetByID(It.IsAny<Guid>())).Throws<ArgumentException>();
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);        

        // Act
        Func<Task> act = async () => await service.CreateBookingAsync(eventId, 2, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        _eventRepository.Verify(o => o.GetByID(eventId), Times.Once);
    }

    [Fact]
    public async Task GetBooking_ByDeletedEventId_SholdThrowsArgumentException()
    {

        // Arrange
        var ev = TestData.GetTestEvent();
        var eventId = ev.Id;
        _eventRepository.Setup(o => o.GetByID(eventId)).Returns(ev);
        _bookingRepository.Setup(o => o.Add(It.IsAny<Booking>())).Returns<Booking>(o => o);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        var result =  await service.CreateBookingAsync(eventId, 2, CancellationToken.None);
        _eventRepository.Setup(o => o.GetByID(It.IsAny<Guid>())).Throws<ArgumentException>();
        Func<Task> act = async () => await service.CreateBookingAsync(eventId, 2, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(eventId);
        await act.Should().ThrowAsync<ArgumentException>();
        _eventRepository.Verify(o => o.GetByID(eventId), Times.Exactly(2));
    }


    [Fact]
    public async Task GetBooking_ByInvalidBookingId_ShouldThrowsArgumentException()
    {
        // Arrange        
        var bookingId = Guid.NewGuid();
        _bookingRepository.Setup(o => o.GetById(It.IsAny<Guid>())).Throws<ArgumentException>();
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        Func<Task> act = async () => await service.GetBookingByIdAsync(bookingId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
    
    
    [Fact]
    public async Task CreateBooking_OneSeat_ReturnReducedSeatsNumber()
    {
        // Arrange        
        var ev = TestData.GetTestEvent(1);
        var id = ev.Id;
        var seats = 1;

        _eventRepository.Setup(o => o.GetByID(id)).Returns(ev);
        _eventRepository.Setup(o => o.Update(ev));
        _bookingRepository.Setup(o => o.Add(It.IsAny<Booking>())).Returns<Booking>(b => b);

        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        var result = await service.CreateBookingAsync(id, seats, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(id);        
        ev.AvailableSeats.Should().Be(0);
        _eventRepository.Verify(o => o.GetByID(id), Times.Once);
        _eventRepository.Verify(o => o.Update(ev), Times.Once);
        _bookingRepository.Verify(o => o.Add(It.IsAny<Booking>()), Times.Once);
    }

    [Fact]
    public async Task CreateSeveralBooking_ReturnBookingsWhitUniqueId()
    {
        // Arrange        
        var ev = TestData.GetTestEvent(3);
        var id = ev.Id;
        var seats = 1;
        var ids = new List<Guid>();
        var cnt = 3;

        _eventRepository.Setup(o => o.GetByID(id)).Returns(ev);
        _eventRepository.Setup(o => o.Update(ev));
        _bookingRepository.Setup(o => o.Add(It.IsAny<Booking>())).Returns<Booking>(b => b);

        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        for (int i = 0; i < cnt; ++i)
        {
            var result = await service.CreateBookingAsync(id, seats, CancellationToken.None);
            ids.Add(result.Id);
        }

        // Assert
        ids.Should().HaveCount(cnt);
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task CreateSeveralBooking_ExecuteCountMoreThenTotalSeats_ThrowsNoAvailableSeatsException()
    {
        // Arrange        
        var ev = TestData.GetTestEvent(3);
        var id = ev.Id;
        var seats = 1;
        var ids = new List<Guid>();
        var cnt = 3;

        _eventRepository.Setup(o => o.GetByID(id)).Returns(ev);
        _eventRepository.Setup(o => o.Update(ev));
        _bookingRepository.Setup(o => o.Add(It.IsAny<Booking>())).Returns<Booking>(b => b);

        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        for (int i = 0; i < cnt; ++i)
        {
            var result = await service.CreateBookingAsync(id, seats, CancellationToken.None);
            ids.Add(result.Id);
        }
        Func<Task<BookingResponseDTO>> act = async () => await service.CreateBookingAsync(id, seats, CancellationToken.None);

        // Assert
        ids.Should().HaveCount(cnt);
        ids.Should().OnlyHaveUniqueItems();
        await act.Should().ThrowAsync<NoAvailableSeatsException>();
    }
}
