using Contracts;

namespace Events.Application.Interfaces.MessageHandlers;


public interface IBookingConfirmedHandler
{
    Task HandleMessage(BookingConfirmed message);
}
