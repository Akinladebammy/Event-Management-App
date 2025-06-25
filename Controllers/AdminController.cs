using EventManagementApp.DTOs;
using EventManagementApp.DTOs.EventManagementApp.DTOs;
using EventManagementApp.Models.Entities;
using EventManagementApp.Models.Enum;
using EventManagementApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace EventManagementApp.Controllers
{
    // Remove the [Authorize] attribute since we're using session-based auth
    public class AdminController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;
        private readonly IUserService _userService;

        public AdminController(IEventService eventService, IBookingService bookingService, IUserService userService)
        {
            _eventService = eventService;
            _bookingService = bookingService;
            _userService = userService;
        }

        // Helper method to check if user is admin
        private bool IsUserAdmin()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == Role.Admin.ToString();
        }

        // Helper method to redirect non-admin users
        private IActionResult RedirectIfNotAdmin()
        {
            if (!IsUserAdmin())
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }
            return null;
        }

        // GET: /Admin/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            //var authCheck = RedirectIfNotAdmin();
            //if (authCheck != null) return authCheck;

            try
            {
                var events = await _eventService.GetAllEventsAsync();
                var totalEvents = events?.Count() ?? 0;
                var totalActiveEvents = events?.Count(e => e.Date > DateTime.Now) ?? 0;

                var recentEvents = new List<EventDto>();

                if (events != null && events.Any())
                {
                    foreach (var eventItem in events.OrderByDescending(e => e.Date).Take(5))
                    {
                        var availableSpots = await _eventService.GetAvailableSpotsAsync(eventItem.Id);

                        recentEvents.Add(new EventDto
                        {
                            Id = eventItem.Id,
                            Title = eventItem.Title,
                            Description = eventItem.Description,
                            Date = eventItem.Date,
                            Location = eventItem.Location,
                            Capacity = eventItem.Capacity,
                            AvailableSpots = availableSpots
                        });
                    }
                }

                var dashboardData = new AdminDashboardDto
                {
                    TotalEvents = totalEvents,
                    ActiveEvents = totalActiveEvents,
                    CompletedEvents = totalEvents - totalActiveEvents,
                    RecentEvents = recentEvents ?? new List<EventDto>()
                };

                if (TempData["SuccessMessage"] == null)
                {
                    ViewBag.WelcomeMessage = $"Welcome to the Admin Dashboard, {HttpContext.Session.GetString("Username") ?? "Admin"}!";
                }

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading dashboard: " + ex.Message;

                var errorModel = new AdminDashboardDto
                {
                    TotalEvents = 0,
                    ActiveEvents = 0,
                    CompletedEvents = 0,
                    RecentEvents = new List<EventDto>()
                };

                return View(errorModel);
            }
        }

        // GET: /Admin/Events
        [HttpGet]
        public async Task<IActionResult> Events()
        {
            var authCheck = RedirectIfNotAdmin();
            if (authCheck != null) return authCheck;

            try
            {
                var events = await _eventService.GetAllEventsAsync();
                return View(events);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading events: " + ex.Message;
                return View(new List<EventDto>());
            }
        }

        // GET: /Admin/CreateEvent
        [HttpGet]
        public IActionResult CreateEvent()
        {
            var authCheck = RedirectIfNotAdmin();
            if (authCheck != null) return authCheck;

            // Return a new EventDto instead of nothing
            var eventDto = new EventDto();
            return View(eventDto);
        }

        // POST: /Admin/CreateEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventDto eventDto)
        {
            var authCheck = RedirectIfNotAdmin();
            if (authCheck != null) return authCheck;

            if (!ModelState.IsValid)
            {
                return View(eventDto); // Return the EventDto, not Event
            }

            try
            {
                // Convert EventDto to Event entity for the service
                var eventModel = new Event
                {
                    Id = Guid.NewGuid(),
                    Title = eventDto.Title,
                    Description = eventDto.Description,
                    Date = eventDto.Date,
                    Location = eventDto.Location,
                    Capacity = eventDto.Capacity
                };

                await _eventService.CreateEventAsync(eventModel);
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Events));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating event: " + ex.Message);
                return View(eventDto); // Return the EventDto, not Event
            }
        }

        // GET: /Admin/EditEvent/{id}
        [HttpGet]
        public async Task<IActionResult> EditEvent(Guid id)
        {
            var authCheck = RedirectIfNotAdmin();
            if (authCheck != null) return authCheck;

            try
            {
                var eventDto = await _eventService.GetEventByIdAsync(id);
                if (eventDto == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction(nameof(Events));
                }

                var eventModel = new Event
                {
                    Id = eventDto.Id,
                    Title = eventDto.Title,
                    Description = eventDto.Description,
                    Date = eventDto.Date,
                    Location = eventDto.Location,
                    Capacity = eventDto.Capacity
                };

                return View(eventModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading event: " + ex.Message;
                return RedirectToAction(nameof(Events));
            }
        }

        // POST: /Admin/EditEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(Event eventModel)
        {
            var authCheck = RedirectIfNotAdmin();
            if (authCheck != null) return authCheck;

            if (!ModelState.IsValid)
            {
                return View(eventModel);
            }

            try
            {
                await _eventService.UpdateEventAsync(eventModel);
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction(nameof(Events));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating event: " + ex.Message);
                return View(eventModel);
            }
        }

        // POST: /Admin/DeleteEvent/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var authCheck = RedirectIfNotAdmin();
            if (authCheck != null) return authCheck;

            try
            {
                var result = await _eventService.DeleteEventAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Event deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete event.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting event: " + ex.Message;
            }

            return RedirectToAction(nameof(Events));
        }

        // GET: /Admin/Bookings
        [HttpGet]
        public async Task<IActionResult> Bookings()
        {
            var authCheck = RedirectIfNotAdmin();
            if (authCheck != null) return authCheck;

            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();

                // Convert to List and sort by BookingDate descending
                var bookingsList = bookings?.OrderByDescending(b => b.BookingDate).ToList() ?? new List<AdminBookingDto>();

                return View(bookingsList);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading bookings: " + ex.Message;
                return View(new List<AdminBookingDto>());
            }
        }

        // GET: /Admin/Users
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var authCheck = RedirectIfNotAdmin();
            if (authCheck != null) return authCheck;

            try
            {
                var users = await _userService.GetAllUsersAsync();
                // Convert to List to match the view model expectation
                var usersList = users?.ToList() ?? new List<AdminUserDto>();
                return View(usersList);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading users: " + ex.Message;
                return View(new List<AdminUserDto>());
            }
        }
        // POST: /Admin/UpdateUserRole/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(Guid id, Role role)
        {
            var authCheck = RedirectIfNotAdmin();
            if (authCheck != null) return authCheck;

            try
            {
                var result = await _userService.UpdateUserRoleAsync(id, role);
                if (result)
                {
                    TempData["SuccessMessage"] = "User role updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update user role.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating user role: " + ex.Message;
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/DeleteUser/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var authCheck = RedirectIfNotAdmin();
            if (authCheck != null) return authCheck;

            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "User deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete user.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting user: " + ex.Message;
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/CancelBooking/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var authCheck = RedirectIfNotAdmin();
            if (authCheck != null) return authCheck;

            try
            {
                var result = await _bookingService.CancelBookingByAdminAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Booking cancelled successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to cancel booking.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error cancelling booking: " + ex.Message;
            }

            return RedirectToAction(nameof(Bookings));
        }

        // GET: /Admin/EventDetails/{id}
        [HttpGet]
        public async Task<IActionResult> EventDetails(Guid id)
        {
            var authCheck = RedirectIfNotAdmin();
            if (authCheck != null) return authCheck;

            try
            {
                var eventDto = await _eventService.GetEventByIdAsync(id);
                if (eventDto == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction(nameof(Events));
                }

                return View(eventDto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading event details: " + ex.Message;
                return RedirectToAction(nameof(Events));
            }
        }

        // GET: /Admin/EventBookings/{eventId}
        [HttpGet]
        public async Task<IActionResult> EventBookings(Guid eventId)
        {
            var authCheck = RedirectIfNotAdmin();
            if (authCheck != null) return authCheck;

            try
            {
                var eventDto = await _eventService.GetEventByIdAsync(eventId);
                if (eventDto == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction(nameof(Events));
                }

                var bookings = await _bookingService.GetEventBookingsAsync(eventId);
                ViewBag.EventTitle = eventDto.Title;
                ViewBag.EventId = eventId;

                return View(bookings);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading event bookings: " + ex.Message;
                return RedirectToAction(nameof(Events));
            }
        }
    }

    // DTO for admin dashboard
    public class AdminDashboardDto
    {
        public int TotalEvents { get; set; } = 0;
        public int ActiveEvents { get; set; } = 0;
        public int CompletedEvents { get; set; } = 0;
        public List<EventDto> RecentEvents { get; set; } = new List<EventDto>();
    }

    // DTO for admin user management
    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalBookings { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    // DTO for admin booking management
    public class AdminBookingDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public string EventTitle { get; set; }
        public string EventDescription { get; set; }
        public DateTime EventDate { get; set; }
        public string EventLocation { get; set; }
        public string Username { get; set; }
        public string UserEmail { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
    }
}