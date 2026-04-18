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
    private readonly Mock<IBookingRepository> _repository;

    public BookingTest()
    {
        _repository = new Mock<IBookingRepository>();
    }

    //[Fact]
    //public async Task  CreateBooking_ByEventId_ReturnBookingWithPendingStatus()
    //{
    //    // Arrange        
    //    var service = new BookingService(new BookingRepository());
    //    var eventID = Guid.NewGuid();

    //    // Act
    //    var result = await  service.CreateBookingAsync(eventID, CancellationToken.None);

    //    // Assert
    //    result.EventId.Should().Be(eventID);
    //    result.Status.Should().Be(BookingStatus.Pending);
    //}

    //[Fact]
    //public async Task CreateSeveralBookings_ByEventID_ReturnsUniqueBookingId()
    //{
    //    // Arrange        
    //    var service = new BookingService(new BookingRepository());
    //    var eventID = Guid.NewGuid();
    //    var ids = new List<Guid>();

    //    // Act
    //    for (int i = 0; i < 3; ++i)
    //    {            
    //        var result = await service.CreateBookingAsync(eventID, CancellationToken.None);
    //        ids.Add(result.Id);
    //    }

    //    // Assert
    //    ids.Should().HaveCount(3);
    //    ids.Should().OnlyHaveUniqueItems();
    //}

    //[Fact]
    //public async Task GetBooking_ById_ReturnBookig()
    //{
    //    // Arrange                
    //    var service = getBookingService(TestData.GetBookingTestData());
    //    var booking = TestData.GetTestBookings().First();

    //    // Act
    //    var result = await service.GetBookingByIdAsync(booking.Id, CancellationToken.None);

    //    // Assert
    //    result.EventId.Should().Be(booking.EventId);
    //    result.Id.Should().Be(booking.Id);
    //    result.Status.Should().Be(booking.Status);
    //}

    //[Fact]
    //public async Task GetBooking_ChangeStatus_ReturnChangedStatus()
    //{
    //    // Arrange
    //    var rep = new Mock<IBookingRepository>();
    //    var data = TestData.GetBookingTestData();        
    //    var booking = TestData.GetTestBookings().First();
    //    var service = getBookingService(data);
    //    var id = data.First().Key;

    //    // Act
    //    var result = await service.GetBookingByIdAsync(id, CancellationToken.None);
    //    data.First(o => o.Key == id).Value.Status = BookingStatus.Rejected;
    //    var result1 = await service.GetBookingByIdAsync(id, CancellationToken.None);

    //    // Assert
    //    result.Status.Should().NotBe(result1.Status);
    //}

    //[Fact]
    //public async Task GetBooking_ByInvalidEventId_SholdThrowsBookingValidationException()
    //{
    //    // Arrange
    //    var data = TestData.GetBookingTestData();

    //    var service = new BookingService(new BookingRepository(), new BookingValidator(new EventRepository()));
    //    var eventID = Guid.NewGuid();

    //    // Act
    //    Func<Task> act = async () => await service.CreateBookingAsync(eventID, CancellationToken.None);

    //    // Assert
    //    await act.Should().ThrowAsync<BookingValidationException>();
    //}

    //[Fact]
    //public async Task GetBooking_ByDeletedEventId_SholdThrowsBookingValidationException()
    //{
    //    // Arrange
    //    var bookingData = TestData.GetBookingTestData();
    //    var events = TestData.GetTestData();
    //    var eventId = events.First().Key;
    //    var eventData = new ConcurrentDictionary<Guid, Event>(events);
    //    var eventRepository = new Mock<IEventRepository>();
    //    eventRepository.Setup(r => r.Data).Returns(() => eventData);
    //    _repository.Setup(r => r.Bookings).Returns(() => new ConcurrentDictionary<Guid, Booking>(bookingData));
    //    var service = new BookingService(_repository.Object, new BookingValidator(eventRepository.Object));        

    //    // Act
    //    var result = await service.CreateBookingAsync(eventId, CancellationToken.None);
    //    eventData.TryRemove(eventId, out var deletedEvent);
    //    Func<Task> act = async () => await service.CreateBookingAsync(eventId, CancellationToken.None);
                
    //    // Assert
    //    result.EventId.Should().Be(eventId);
    //    result.Status.Should().Be(BookingStatus.Pending);

    //    await act.Should().ThrowAsync<BookingValidationException>();
    //}


    //[Fact]
    //public async Task GetBooking_ByInvalidBookingId_ShouldThrowsArgumentException()
    //{
    //    // Arrange
    //    var service = getBookingService(TestData.GetBookingTestData());
    //    var bookingId = Guid.NewGuid();

    //    // Act
    //    Func<Task> act = async () => await service.GetBookingByIdAsync(bookingId, CancellationToken.None);

    //    // Assert
    //    await act.Should().ThrowAsync<ArgumentException>();
    //}


    //private BookingService getBookingService(List<KeyValuePair<Guid, Booking>> data)
    //{
    //    _repository.Setup(r => r.Bookings).Returns(() => new ConcurrentDictionary<Guid, Booking>(data));
    //    return new BookingService(_repository.Object);
    //}
}
