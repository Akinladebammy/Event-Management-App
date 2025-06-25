using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagementApp.DTOs;
using EventManagementApp.Models.Entities;
using EventManagementApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementApp.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        // GET: /Events
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get current user from session
            var userIdString = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("Username");

            Guid? currentUserId = null;
            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out Guid userId))
            {
                currentUserId = userId;
            }

            // Pass user info to ViewBag for the view
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.Username = username;

            var events = await _eventService.GetAllEventsAsync(currentUserId);
            return View(events);
        }

        // GET: /Events/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            // Get current user from session
            var userIdString = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("Username");

            Guid? currentUserId = null;
            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out Guid userId))
            {
                currentUserId = userId;
            }

            // Pass user info to ViewBag for the view
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.Username = username;

            var eventDto = await _eventService.GetEventByIdAsync(id, currentUserId);
            if (eventDto == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(eventDto);
        }

        // GET: /Events/Create (Admin only - should probably be moved to AdminController)
        [HttpGet]
        public IActionResult Create()
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // POST: /Events/Create (Admin only - should probably be moved to AdminController)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventItem)
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                eventItem.Id = Guid.NewGuid(); // Ensure ID is set
                await _eventService.CreateEventAsync(eventItem);
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(eventItem);
        }

        // GET: /Events/Edit/{id} (Admin only - should probably be moved to AdminController)
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction(nameof(Index));
            }

            var eventDto = await _eventService.GetEventByIdAsync(id);
            if (eventDto == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            // Convert EventDto to Event for editing
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

        // POST: /Events/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Event eventItem)
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                await _eventService.UpdateEventAsync(eventItem);
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(eventItem);
        }

        // POST: /Events/Delete/{id} (Admin only - should probably be moved to AdminController)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _eventService.DeleteEventAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Event deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete event.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Events/AvailableSpots/{eventId}
        [HttpGet]
        public async Task<JsonResult> GetAvailableSpots(Guid eventId)
        {
            try
            {
                var availableSpots = await _eventService.GetAvailableSpotsAsync(eventId);
                return Json(new { success = true, spots = availableSpots });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}