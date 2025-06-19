using EventManagementApp.DTOs;
using EventManagementApp.DTOs.EventManagementApp.DTOs;
using EventManagementApp.Models.Entities;

namespace EventManagementApp.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDto> BookEventAsync(Guid eventId, Guid userId);
        Task<IEnumerable<BookingDto>> GetUserBookingsAsync(Guid userId);
        Task<bool> CancelBookingAsync(Guid bookingId, Guid userId);
        Task<bool> IsEventBookedByUserAsync(Guid eventId, Guid userId);
    }
}
