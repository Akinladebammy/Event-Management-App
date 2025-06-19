using EventManagementApp.Data;
using EventManagementApp.Models.Entities;
using EventManagementApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApp.Repositories.Implementations
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EventBooking> AddAsync(EventBooking booking)
        {
            _context.EventBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<IEnumerable<EventBooking>> GetByUserIdAsync(Guid userId)
        {
            return await _context.EventBookings
                .Include(eb => eb.Event)
                .Where(eb => eb.UserId == userId)
                .OrderByDescending(eb => eb.BookingDate)
                .ToListAsync();
        }

        public async Task<EventBooking> GetByIdAndUserIdAsync(Guid bookingId, Guid userId)
        {
            return await _context.EventBookings
                .FirstOrDefaultAsync(eb => eb.Id == bookingId && eb.UserId == userId);
        }

        public async Task<bool> DeleteAsync(EventBooking booking)
        {
            _context.EventBookings.Remove(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid eventId, Guid userId)
        {
            return await _context.EventBookings
                .AnyAsync(eb => eb.EventId == eventId && eb.UserId == userId);
        }

        public async Task<int> CountByEventIdAsync(Guid eventId)
        {
            return await _context.EventBookings.CountAsync(eb => eb.EventId == eventId);
        }
    }
}
