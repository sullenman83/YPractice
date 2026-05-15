using EventManagement.Common;
using EventManagement.Common.Exceptions;
using EventManagement.Interfaces;
using EventManagement.Models.BookingModels;
using EventManagement.Models.Events;
using EventManagement.Services;
using EventManagement.Services.BookingServices;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Concurrent;
using Xunit.Abstractions;

namespace EventServiceTest;

public class BookingTest
{
    private readonly Mock<IBookingRepository<Booking>> _bookingRepository;
    private readonly Mock<IEventRepository<Event>> _eventRepository;

    public BookingTest()
    {
        _bookingRepository = new Mock<IBookingRepository<Booking>>();
        _eventRepository = new Mock<IEventRepository<Event>>();
    }

    [Fact]
    public async Task CreateBooking_ByEventId_ReturnsBookingWithPendingStatus()
    {
        // Arrange        
        var totalSeats = 10;
        var ev = TestData.GetTestEvent(totalSeats);
        var id = ev.Id;
        var seats = 5;

        var tr = new Mock<IDbContextTransaction>();                
        _eventRepository.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(tr.Object);
        _eventRepository.Setup(o => o.GetEventWithBlockingAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        tr.Setup(o => o.CommitAsync());
        tr.Setup(o => o.RollbackAsync());        
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        var result = await service.CreateBookingAsync(id, seats, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(id);
        result.Status.Should().Be(BookingStatus.Pending);
        ev.AvailableSeats.Should().Be(ev.TotalSeats - seats);
        _eventRepository.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _eventRepository.Verify(o => o.GetEventWithBlockingAsync(id), Times.Once);
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Once);
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Once);
        tr.Verify(o => o.CommitAsync(), Times.Once);
        tr.Verify(o => o.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateSeveralBookings_ByEventID_ReturnsUniqueBookingId()
    {
        // Arrange        
        var ev = TestData.GetTestEvent();
        var id = ev.Id;
        var ids = new List<Guid>();
        var bookingCount = 3;

        var tr = new Mock<IDbContextTransaction>();
        _eventRepository.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(tr.Object);
        _eventRepository.Setup(o => o.GetEventWithBlockingAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        tr.Setup(o => o.CommitAsync());
        tr.Setup(o => o.RollbackAsync());
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        for (int i = 0; i < bookingCount; ++i)
        {
            var result = await service.CreateBookingAsync(id, 1, CancellationToken.None);
            ids.Add(result.Id);
        }

        // Assert
        ids.Should().HaveCount(bookingCount);
        ids.Should().OnlyHaveUniqueItems();
        _eventRepository.Verify(o => o.BeginTransactionAsync(), Times.Exactly(bookingCount));
        _eventRepository.Verify(o => o.GetEventWithBlockingAsync(id), Times.Exactly(bookingCount));
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Exactly(bookingCount));
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Exactly(bookingCount));
        tr.Verify(o => o.CommitAsync(), Times.Exactly(bookingCount));
        tr.Verify(o => o.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task GetBooking_ById_ReturnsBookig()
    {
        // Arrange
        var eventID = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventID, 1, DateTimeProvider.UtcNow);
        var id = booking.Id;
        _bookingRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(booking);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        var result = await service.GetBookingByIdAsync(booking.Id, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(booking.EventId);
        result.Id.Should().Be(booking.Id);
        result.Status.Should().Be(booking.Status);
    }

    [Fact]
    public async Task GetBooking_ChangeStatus_ReturnsChangedStatus()
    {
        // Arrange        
        var eventID = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventID, 1, DateTimeProvider.UtcNow);
        var id = booking.Id;
        _bookingRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(booking);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);


        // Act
        var result = await service.GetBookingByIdAsync(id, CancellationToken.None);
        booking.Reject();
        var result1 = await service.GetBookingByIdAsync(id, CancellationToken.None);

        // Assert
        result1.Status.Should().NotBe(BookingStatus.Pending);
    }
    
    [Fact]
    public async Task CreateBooking_ByDeletedEventId_ThrowsNotFoundException()
    {
        // Arrange
        var ev = TestData.GetTestEvent();
        var eventId = ev.Id;
        var tr = new Mock<IDbContextTransaction>();
        _eventRepository.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(tr.Object);
        _eventRepository.Setup(o => o.GetEventWithBlockingAsync(eventId)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        var result = await service.CreateBookingAsync(eventId, 2, CancellationToken.None);
        _eventRepository.Setup(o => o.GetEventWithBlockingAsync(It.IsAny<Guid>())).Throws<NotFoundException>();
        Func<Task> act = async () => await service.CreateBookingAsync(eventId, 2, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(eventId);
        await act.Should().ThrowAsync<NotFoundException>();
        _eventRepository.Verify(o => o.BeginTransactionAsync(), Times.Exactly(2));
        _eventRepository.Verify(o => o.GetEventWithBlockingAsync(eventId), Times.Exactly(2));
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Exactly(1));
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Exactly(1));
        tr.Verify(o => o.CommitAsync(), Times.Exactly(1));
        tr.Verify(o => o.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task GetBooking_ByInvalidBookingId_ThrowsNotFoundException()
    {
        // Arrange        
        var bookingId = Guid.NewGuid();
        _bookingRepository.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).Throws<NotFoundException>();
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        Func<Task> act = async () => await service.GetBookingByIdAsync(bookingId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }


    [Fact]
    public async Task CreateBooking_OneSeat_ReturnsReducedSeatsNumber()
    {
        // Arrange        
        var ev = TestData.GetTestEvent(1);
        var id = ev.Id;
        var seats = 1;

        var tr = new Mock<IDbContextTransaction>();
        _eventRepository.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(tr.Object);
        _eventRepository.Setup(o => o.GetEventWithBlockingAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        tr.Setup(o => o.CommitAsync());
        tr.Setup(o => o.RollbackAsync());
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        var result = await service.CreateBookingAsync(id, seats, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(id);
        ev.AvailableSeats.Should().Be(0);
        _eventRepository.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _eventRepository.Verify(o => o.GetEventWithBlockingAsync(id), Times.Once);
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Once);
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Once);
        tr.Verify(o => o.CommitAsync(), Times.Once);
        tr.Verify(o => o.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateSeveralBooking_ReturnsBookingWhitUniqueId()
    {
        // Arrange        
        var ev = TestData.GetTestEvent(3);
        var id = ev.Id;
        var seats = 1;
        var ids = new List<Guid>();
        var cnt = 3;

        var tr = new Mock<IDbContextTransaction>();
        _eventRepository.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(tr.Object);
        _eventRepository.Setup(o => o.GetEventWithBlockingAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        tr.Setup(o => o.CommitAsync());
        tr.Setup(o => o.RollbackAsync());
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
        _eventRepository.Verify(o => o.BeginTransactionAsync(), Times.Exactly(cnt));
        _eventRepository.Verify(o => o.GetEventWithBlockingAsync(id), Times.Exactly(cnt));
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Exactly(cnt));
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Exactly(cnt));
        tr.Verify(o => o.CommitAsync(), Times.Exactly(cnt));
        tr.Verify(o => o.RollbackAsync(), Times.Never);
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

        var tr = new Mock<IDbContextTransaction>();
        _eventRepository.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(tr.Object);
        _eventRepository.Setup(o => o.GetEventWithBlockingAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        tr.Setup(o => o.CommitAsync());
        tr.Setup(o => o.RollbackAsync());
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
        _eventRepository.Verify(o => o.BeginTransactionAsync(), Times.Exactly(cnt + 1));
        _eventRepository.Verify(o => o.GetEventWithBlockingAsync(id), Times.Exactly(cnt + 1));        
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Exactly(cnt));
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Exactly(cnt));
        tr.Verify(o => o.CommitAsync(), Times.Exactly(cnt));
        tr.Verify(o => o.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateBooking_NoAvailableSeats_ThrowsNoAvailableSeatsException()
    {
        //Arrange
        var ev = TestData.GetTestEvent(1);
        var id = ev.Id;
        var seatsCount = 2;

        var tr = new Mock<IDbContextTransaction>();
        _eventRepository.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(tr.Object);
        _eventRepository.Setup(o => o.GetEventWithBlockingAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        tr.Setup(o => o.CommitAsync());
        tr.Setup(o => o.RollbackAsync());
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);        

        //Act
        Func<Task<BookingResponseDTO>> act = async () => await service.CreateBookingAsync(id, seatsCount, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NoAvailableSeatsException>();
        _eventRepository.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _eventRepository.Verify(o => o.GetEventWithBlockingAsync(id), Times.Once);
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Never);
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Never);
        tr.Verify(o => o.CommitAsync(), Times.Never);
        tr.Verify(o => o.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task ChangeBookingStatus_SetConfirm_ReturnChangedBooking()
    {
        // Arrange                
        var eventID = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventID, 1, DateTimeProvider.UtcNow);
        var id = booking.Id;
        _bookingRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(booking);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        var result = await service.GetBookingByIdAsync(id, CancellationToken.None);
        booking.Confirm();
        var result1 = await service.GetBookingByIdAsync(id, CancellationToken.None);

        // Assert
        result.Status.Should().Be(BookingStatus.Pending);
        result1.Status.Should().Be(BookingStatus.Confirmed);
        result1.ProcessedAt.Should().NotBeNull();
        _bookingRepository.Verify(o => o.GetByIdAsync(id), Times.Exactly(2));
    }

    [Fact]
    public async Task ChangeBookingStatus_SetReject_ReturnChangedBooking()
    {
        // Arrange                
        var eventID = Guid.NewGuid();
        var booking = new Booking(BookingStatus.Pending, eventID, 1, DateTimeProvider.UtcNow);
        var id = booking.Id;
        _bookingRepository.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(booking);
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);


        // Act
        var result = await service.GetBookingByIdAsync(id, CancellationToken.None);
        booking.Reject();
        var result1 = await service.GetBookingByIdAsync(id, CancellationToken.None);

        // Assert
        result.Status.Should().Be(BookingStatus.Pending);
        result1.Status.Should().Be(BookingStatus.Rejected);
        result1.ProcessedAt.Should().NotBeNull();
        _bookingRepository.Verify(o => o.GetByIdAsync(id), Times.Exactly(2));
    }

    [Fact]
    public async Task ReleaseSeatsAfterReject_ReturnRightAvailableSeats()
    {
        // Arrange        
        int cnt = 5;
        var ev = TestData.GetTestEvent(10);
        var booking = new Booking(BookingStatus.Confirmed, Guid.NewGuid(), cnt, DateTimeProvider.UtcNow);
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
        var booking = new Booking(BookingStatus.Confirmed, Guid.NewGuid(), cnt, DateTimeProvider.UtcNow);
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
        var id = ev.Id;
        var tasks = new List<Task<BookingResponseDTO>>();
        var bookingCnt = 1;

        var tr = new Mock<IDbContextTransaction>();
        _eventRepository.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(tr.Object);
        _eventRepository.Setup(o => o.GetEventWithBlockingAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        tr.Setup(o => o.CommitAsync());
        tr.Setup(o => o.RollbackAsync());
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
        catch (Exception)
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
        var id = ev.Id;
        var tasks = new List<Task<BookingResponseDTO>>();
        var bookingCnt = 1;

        var tr = new Mock<IDbContextTransaction>();
        _eventRepository.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(tr.Object);
        _eventRepository.Setup(o => o.GetEventWithBlockingAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        tr.Setup(o => o.CommitAsync());
        tr.Setup(o => o.RollbackAsync());
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        for (int i = 0; i < requestCnt; ++i)
        {
            tasks.Add(Task.Run(async () => await service.CreateBookingAsync(ev.Id, bookingCnt, CancellationToken.None)));
        }
        var task = Task.WhenAll(tasks);

        // Act        
        var result = await task;

        // Assert
        result.Should().HaveCount(requestCnt);
        result.Should().OnlyHaveUniqueItems(o => o.Id);
    }


    [Fact]
    public async Task CreateBooking_NegativeSeatsCount_ThrowsArgumentException()
    {
        // Arrange
        int totalSeats = 10;
        var ev = TestData.GetTestEvent(totalSeats);
        var id = ev.Id;
        var bookingCnt = -1;

        var tr = new Mock<IDbContextTransaction>();
        _eventRepository.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(tr.Object);
        _eventRepository.Setup(o => o.GetEventWithBlockingAsync(id)).ReturnsAsync(ev);
        _eventRepository.Setup(o => o.SaveChangesAsync());
        _bookingRepository.Setup(o => o.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b, CancellationToken t) => b);
        tr.Setup(o => o.CommitAsync());
        tr.Setup(o => o.RollbackAsync());
        var service = new BookingService(_bookingRepository.Object, _eventRepository.Object);

        // Act
        Func<Task<BookingResponseDTO>> act = async () => await service.CreateBookingAsync(id, bookingCnt, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        _eventRepository.Verify(o => o.BeginTransactionAsync(), Times.Once);
        _eventRepository.Verify(o => o.GetEventWithBlockingAsync(id), Times.Once);
        _bookingRepository.Verify(o => o.AddAsync(It.IsAny<Booking>()), Times.Never);
        _eventRepository.Verify(o => o.SaveChangesAsync(), Times.Never);
        tr.Verify(o => o.CommitAsync(), Times.Never);
        tr.Verify(o => o.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task ReleaseSeats_MoreThenTotalSeats_ThrowsInvalidOperationException()
    {
        // Arrange
        int totalSeats = 10;
        var ev = TestData.GetTestEvent(totalSeats);
        var seatsCount = 1;

        // Act
        ev.TryReserveSeats(seatsCount);
        var result1 = ev.ReleaseSeats(seatsCount);
        var result2 = ev.ReleaseSeats(seatsCount);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
    }
}