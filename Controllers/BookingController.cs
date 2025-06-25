using EventManagementApp.DTOs;
using EventManagementApp.DTOs.EventManagementApp.DTOs;
using EventManagementApp.Models.Entities;
using EventManagementApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagementApp.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // POST: /Booking/BookEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookEvent(Guid eventId, Guid userId)
        {
            // Verify the user is logged in and the userId matches the session
            var sessionUserIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(sessionUserIdString))
            {
                TempData["ErrorMessage"] = "You must be logged in to book events.";
                return RedirectToAction("Login", "User", new { returnUrl = Url.Action("Index", "Event") });
            }

            if (!Guid.TryParse(sessionUserIdString, out Guid sessionUserId) || sessionUserId != userId)
            {
                TempData["ErrorMessage"] = "Invalid user session. Please log in again.";
                return RedirectToAction("Login", "User");
            }

            try
            {
                var booking = await _bookingService.BookEventAsync(eventId, userId);
                TempData["SuccessMessage"] = "Event booked successfully! 🎉";
                return RedirectToAction("Index", "Event");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", "Event", new { id = eventId });
            }
        }

        // GET: /Booking/UserBookings/{userId}
        [HttpGet]
        public async Task<IActionResult> UserBookings(Guid userId)
        {
            // Verify the user is logged in and can access this user's bookings
            var sessionUserIdString = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(sessionUserIdString))
            {
                TempData["ErrorMessage"] = "You must be logged in to view bookings.";
                return RedirectToAction("Login", "User");
            }

            if (!Guid.TryParse(sessionUserIdString, out Guid sessionUserId))
            {
                TempData["ErrorMessage"] = "Invalid user session. Please log in again.";
                return RedirectToAction("Login", "User");
            }

            // Allow admins to view any user's bookings, regular users can only view their own
            if (userRole != "Admin" && sessionUserId != userId)
            {
                TempData["ErrorMessage"] = "You can only view your own bookings.";
                return RedirectToAction("Index", "Event");
            }

            try
            {
                var bookings = await _bookingService.GetUserBookingsAsync(userId);

                // Pass user info to ViewBag
                ViewBag.Username = HttpContext.Session.GetString("Username");
                ViewBag.UserId = userId;
                ViewBag.IsOwnBookings = (sessionUserId == userId);

                return View(bookings);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading bookings: " + ex.Message;
                return RedirectToAction("Index", "Event");
            }
        }

        // POST: /Booking/CancelBooking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(Guid bookingId, Guid userId)
        {
            // Verify the user is logged in and the userId matches the session
            var sessionUserIdString = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(sessionUserIdString))
            {
                TempData["ErrorMessage"] = "You must be logged in to cancel bookings.";
                return RedirectToAction("Login", "User");
            }

            if (!Guid.TryParse(sessionUserIdString, out Guid sessionUserId))
            {
                TempData["ErrorMessage"] = "Invalid user session. Please log in again.";
                return RedirectToAction("Login", "User");
            }

            // Allow admins to cancel any booking, regular users can only cancel their own
            if (userRole != "Admin" && sessionUserId != userId)
            {
                TempData["ErrorMessage"] = "You can only cancel your own bookings.";
                return RedirectToAction("UserBookings", new { userId = sessionUserId });
            }

            try
            {
                var result = await _bookingService.CancelBookingAsync(bookingId, userId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Booking cancelled successfully! 😊";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to cancel booking. It may have already been cancelled.";
                }
                return RedirectToAction("UserBookings", new { userId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error cancelling booking: " + ex.Message;
                return RedirectToAction("UserBookings", new { userId });
            }
        }

        // GET: /Booking/IsBooked/{eventId}/{userId} - AJAX endpoint
        [HttpGet]
        public async Task<JsonResult> IsEventBookedByUser(Guid eventId, Guid userId)
        {
            try
            {
                // Verify the requesting user has permission to check this
                var sessionUserIdString = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(sessionUserIdString))
                {
                    return Json(new { error = "User not logged in" });
                }

                if (!Guid.TryParse(sessionUserIdString, out Guid sessionUserId))
                {
                    return Json(new { error = "Invalid user session" });
                }

                // Allow admins to check any user, regular users can only check themselves
                if (userRole != "Admin" && sessionUserId != userId)
                {
                    return Json(new { error = "Permission denied" });
                }

                var result = await _bookingService.IsEventBookedByUserAsync(eventId, userId);
                return Json(new { success = true, isBooked = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // GET: /Booking/MyBookings - Convenience method for current user
        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                TempData["ErrorMessage"] = "You must be logged in to view your bookings.";
                return RedirectToAction("Login", "User");
            }

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                TempData["ErrorMessage"] = "Invalid user session. Please log in again.";
                return RedirectToAction("Login", "User");
            }

            return RedirectToAction("UserBookings", new { userId });
        }
    }
}