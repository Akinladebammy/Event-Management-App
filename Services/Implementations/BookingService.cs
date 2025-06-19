using EventManagementApp.Data;
using EventManagementApp.DTOs;
using EventManagementApp.DTOs.EventManagementApp.DTOs;
using EventManagementApp.Models.Entities;
using EventManagementApp.Repositories.Interfaces;
using EventManagementApp.Services.Interfaces;

namespace EventManagementApp.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;

        public BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository, IUserRepository userRepository)
        {
            _bookingRepository = bookingRepository;
            _eventRepository = eventRepository;
            _userRepository = userRepository;
        }

        public async Task<BookingDto> BookEventAsync(Guid eventId, Guid userId)
        {
            // Check if event exists
            var eventItem = await _eventRepository.GetByIdAsync(eventId);
            if (eventItem == null)
                throw new InvalidOperationException("Event not found.");

            // Check if user already booked this event
            if (await _bookingRepository.ExistsAsync(eventId, userId))
                throw new InvalidOperationException("You have already booked this event.");

            // Check capacity
            var currentBookings = await _bookingRepository.CountByEventIdAsync(eventId);
            if (currentBookings >= eventItem.Capacity)
                throw new InvalidOperationException("Event is fully booked.");

            // Get user details
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            var booking = new EventBooking
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                BookingDate = DateTime.UtcNow
            };

            var savedBooking = await _bookingRepository.AddAsync(booking);

            return new BookingDto
            {
                Id = savedBooking.Id,
                EventId = savedBooking.EventId,
                UserId = savedBooking.UserId,
                BookingDate = savedBooking.BookingDate,
                EventTitle = eventItem.Title,
                EventDescription = eventItem.Description,
                EventDate = eventItem.Date,
                EventLocation = eventItem.Location,
                EventCapacity = eventItem.Capacity,
                Username = user.Username,
                UserEmail = user.Email,
                CanCancel = CanCancelBooking(eventItem.Date),
                Status = GetBookingStatus(eventItem.Date)
            };
        }

        public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(Guid userId)
        {
            var bookings = await _bookingRepository.GetByUserIdAsync(userId);
            var user = await _userRepository.GetByIdAsync(userId);

            var bookingDtos = new List<BookingDto>();

            foreach (var booking in bookings)
            {
                var eventItem = booking.Event;

                bookingDtos.Add(new BookingDto
                {
                    Id = booking.Id,
                    EventId = booking.EventId,
                    UserId = booking.UserId,
                    BookingDate = booking.BookingDate,
                    EventTitle = eventItem?.Title ?? "Unknown Event",
                    EventDescription = eventItem?.Description ?? "",
                    EventDate = eventItem?.Date ?? DateTime.MinValue,
                    EventLocation = eventItem?.Location ?? "",
                    EventCapacity = eventItem?.Capacity ?? 0,
                    Username = user?.Username ?? "Unknown User",
                    UserEmail = user?.Email ?? "",
                    CanCancel = eventItem != null ? CanCancelBooking(eventItem.Date) : false,
                    Status = eventItem != null ? GetBookingStatus(eventItem.Date) : "Unknown"
                });
            }

            return bookingDtos.OrderByDescending(b => b.BookingDate);
        }

        public async Task<bool> CancelBookingAsync(Guid bookingId, Guid userId)
        {
            var booking = await _bookingRepository.GetByIdAndUserIdAsync(bookingId, userId);
            if (booking == null) return false;

            return await _bookingRepository.DeleteAsync(booking);
        }

        public async Task<bool> IsEventBookedByUserAsync(Guid eventId, Guid userId)
        {
            return await _bookingRepository.ExistsAsync(eventId, userId);
        }

        private bool CanCancelBooking(DateTime eventDate)
        {
            return eventDate > DateTime.UtcNow.AddHours(24);
        }

        private string GetBookingStatus(DateTime eventDate)
        {
            var now = DateTime.UtcNow;

            if (eventDate < now)
                return "Completed";
            else if (eventDate <= now.AddHours(24))
                return "Upcoming";
            else
                return "Active";
        }
    }
}
