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
        var booking = new Booking(BookingStatus.Pending, eventID, 1, DateTimeOffset.UtcNow);
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
        var booking = new Booking(BookingStatus.Pending, eventID, 1, DateTimeOffset.UtcNow);
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
    public async Task GetBooking_ByInvalidEventId_SholdThrowsNotFoundException()
    {
        // Arrange        
        var eventId = Guid.NewGuid();
        _eventRepository.Setup(o => o.GetByID(It.IsAny<Guid>())).Throws<NotFoundException>();
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        Func<Task> act = async () => await service.CreateBookingAsync(eventId, 2, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _eventRepository.Verify(o => o.GetByID(eventId), Times.Once);
    }

    [Fact]
    public async Task GetBooking_ByDeletedEventId_SholdThrowsNotFoundException()
    {

        // Arrange
        var ev = TestData.GetTestEvent();
        var eventId = ev.Id;
        _eventRepository.Setup(o => o.GetByID(eventId)).Returns(ev);
        _bookingRepository.Setup(o => o.Add(It.IsAny<Booking>())).Returns<Booking>(o => o);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        var result = await service.CreateBookingAsync(eventId, 2, CancellationToken.None);
        _eventRepository.Setup(o => o.GetByID(It.IsAny<Guid>())).Throws<NotFoundException>();
        Func<Task> act = async () => await service.CreateBookingAsync(eventId, 2, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(eventId);
        await act.Should().ThrowAsync<NotFoundException>();
        _eventRepository.Verify(o => o.GetByID(eventId), Times.Exactly(2));
    }


    [Fact]
    public async Task GetBooking_ByInvalidBookingId_ShouldThrowsNotFoundException()
    {
        // Arrange        
        var bookingId = Guid.NewGuid();
        _bookingRepository.Setup(o => o.GetById(It.IsAny<Guid>())).Throws<NotFoundException>();
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        Func<Task> act = async () => await service.GetBookingByIdAsync(bookingId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
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

    [Fact]
    public async Task CreateBooking_NoAvailableSeats_ThrowsNoAvailableSeatsException()
    {
        //Arrange
        var ev = TestData.GetTestEvent(0);
        var id = ev.Id;
        _eventRepository.Setup(o => o.GetByID(id)).Returns(ev);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        //Act
        Func<Task<BookingResponseDTO>> act = async () => await service.CreateBookingAsync(id, 1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NoAvailableSeatsException>();
    }

    [Fact]
    public async Task CreateBooking_SetConfirm_ReturnChangedBooking()
    {
        // Arrange                
        var eventID = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventID, 1, DateTimeOffset.UtcNow);
        var id = booking.Id;
        _bookingRepository.Setup(o => o.GetById(id)).Returns(booking);

        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);


        // Act
        var result = await service.GetBookingByIdAsync(id, CancellationToken.None);
        booking.Confirm();
        var result1 = await service.GetBookingByIdAsync(id, CancellationToken.None);

        // Assert
        result.Status.Should().Be(BookingStatus.Pending);
        result1.Status.Should().Be(BookingStatus.Confirmed);
        result1.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateBooking_SetReject_ReturnChangedBooking()
    {
        // Arrange                
        var eventID = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventID, 1, DateTimeOffset.UtcNow);
        var id = booking.Id;
        _bookingRepository.Setup(o => o.GetById(id)).Returns(booking);

        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);


        // Act
        var result = await service.GetBookingByIdAsync(id, CancellationToken.None);
        booking.Reject();
        var result1 = await service.GetBookingByIdAsync(id, CancellationToken.None);

        // Assert
        result.Status.Should().Be(BookingStatus.Pending);
        result1.Status.Should().Be(BookingStatus.Rejected);
        result1.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ReleaseSeatsAfteReject_ReturnRightAvailableSeats()
    {
        // Arrange        
        int cnt = 5;
        var ev = TestData.GetTestEvent(10);
        var booking = new Booking(BookingStatus.Confirmed, Guid.NewGuid(), cnt, DateTimeOffset.UtcNow);
        ev.TryReserveSeats(cnt);
        var availableSeats = ev.AvailableSeats;
        
        // Act        
        booking.Reject();
        ev.ReleaseSeats(cnt);

        // Assert
        booking.Status.Should().Be(BookingStatus.Rejected);
        availableSeats.Should().Be(ev.TotalSeats - cnt);
        ev.AvailableSeats.Should().Be(ev.TotalSeats);
    }

    [Fact]
    public async Task BookingSeatsAfterRelease()
    {
        // Arrange        
        int cnt = 5;
        var ev = TestData.GetTestEvent(10);
        var booking = new Booking(BookingStatus.Confirmed, Guid.NewGuid(), cnt, DateTimeOffset.UtcNow);
        ev.TryReserveSeats(cnt);
        var availableSeats = ev.AvailableSeats;

        // Act        
        booking.Reject();
        ev.ReleaseSeats(cnt);
        var availableSeats1 = ev.AvailableSeats;
        ev.TryReserveSeats(cnt);        

        // Assert
        booking.Status.Should().Be(BookingStatus.Rejected);
        availableSeats.Should().Be(ev.TotalSeats - cnt);        
        availableSeats1.Should().Be(ev.TotalSeats);
        ev.AvailableSeats.Should().Be(ev.TotalSeats - cnt);
    }

    [Fact]
    public async Task OverbookingProtectionTest_ReturnRightSuccessBooking()
    {
        // Arrange
        int totalSeats = 5;        
        int requestCnt = 20;
        var ev = TestData.GetTestEvent(totalSeats);
        var tasks = new List<Task<BookingResponseDTO>>();
        var bookingCnt = 1;

        _eventRepository.Setup(o => o.GetByID(ev.Id)).Returns(ev);
        _bookingRepository.Setup(o => o.Add(It.IsAny<Booking>())).Returns<Booking>(b => b);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);
        var noAvailableSeatsExceptionCount = 0;

        for (int i = 0; i < requestCnt; ++i)
        {
            tasks.Add(Task.Run(async () => await service.CreateBookingAsync(ev.Id, bookingCnt, CancellationToken.None)));
        }
        var task = Task.WhenAll(tasks);

        // Act
        try
        {
            await task;
        }
        catch(Exception)
        {
            noAvailableSeatsExceptionCount = task.Exception?.InnerExceptions.OfType<NoAvailableSeatsException>().Count()
                ?? throw new Exception("Что-то работает не так");
        }

        var success = tasks.Where(t => t.Status == TaskStatus.RanToCompletion).Count();
        var failed = tasks.Where(t => t.Status == TaskStatus.Faulted).Count();

        // Assert
        success.Should().Be(totalSeats);
        failed.Should().Be(requestCnt - totalSeats);
        ev.AvailableSeats.Should().Be(0);
        noAvailableSeatsExceptionCount.Should().Be(requestCnt - totalSeats);
    }


    [Fact]
    public async Task ConcurentBooking_ReturnUniqueId()
    {
        // Arrange
        int totalSeats = 10;
        int requestCnt = 10;
        var ev = TestData.GetTestEvent(totalSeats);
        var tasks = new List<Task<BookingResponseDTO>>();
        var bookingCnt = 1;

        _eventRepository.Setup(o => o.GetByID(ev.Id)).Returns(ev);
        _bookingRepository.Setup(o => o.Add(It.IsAny<Booking>())).Returns<Booking>(b => b);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);
        
        for (int i = 0; i < requestCnt; ++i)
        {
            tasks.Add(Task.Run(async () => await service.CreateBookingAsync(ev.Id, bookingCnt, CancellationToken.None)));
        }
        var task = Task.WhenAll(tasks);

        // Act        
        var result = await task;        
        
        // Assert
        result.Should().OnlyHaveUniqueItems(o => o.Id);
    }

}