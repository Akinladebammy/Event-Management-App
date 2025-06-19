using EventManagementApp.Models.Entities;

namespace EventManagementApp.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<EventBooking> AddAsync(EventBooking booking);
        Task<IEnumerable<EventBooking>> GetByUserIdAsync(Guid userId);
        Task<EventBooking> GetByIdAndUserIdAsync(Guid bookingId, Guid userId);
        Task<bool> DeleteAsync(EventBooking booking);
        Task<bool> ExistsAsync(Guid eventId, Guid userId);
        Task<int> CountByEventIdAsync(Guid eventId);
    }
}
