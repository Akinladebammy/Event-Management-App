using EventManagementApp.DTOs;
using EventManagementApp.Models.Entities;
using EventManagementApp.Models.Enum;
using EventManagementApp.Services.Implementations;
using EventManagementApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagementApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /Users/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Users/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            try
            {
                var user = await _userService.RegisterAsync(registerDto);
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(registerDto);
            }
        }

        // GET: /Users/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto loginDto, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(loginDto);
            }

            try
            {
                var user = await _userService.LoginAsync(loginDto);

                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    ViewBag.ReturnUrl = returnUrl;
                    return View(loginDto);
                }

                // Set session or authentication here
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserRole", user.Role.ToString());

                // Role-based redirection
                if (user.Role == Role.Admin)
                {
                    TempData["SuccessMessage"] = $"Welcome back, {user.Username}! You're now logged in as Admin.";
                    return RedirectToAction("Dashboard", "Admin");
                }

                // Handle return URL or default redirect for regular users
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Default redirect for regular users - now goes to Events page
                TempData["SuccessMessage"] = $"Welcome back, {user.Username}! Ready to explore some events?";
                return RedirectToAction("Index", "Event");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                ViewBag.ReturnUrl = returnUrl;
                return View(loginDto);
            }
        }

        // GET: /Users/Profile/{id}
        [HttpGet]
        public async Task<IActionResult> Profile(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // GET: /Users/Profile/Username/{username}
        [HttpGet]
        public async Task<IActionResult> ProfileByUsername(string username)
        {
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            return View("Profile", user);
        }

        // POST: /Users/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home"); // Redirect to events page after logout
        }

        [HttpGet]
        public IActionResult AdminDashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != Role.Admin.ToString())
            {
                return Unauthorized();
            }

            // Redirect to the proper Admin Dashboard that has the correct model
            return RedirectToAction("Dashboard", "Admin");
        }

        // GET: /Users/Dashboard - User's personal dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Please log in to access your dashboard.";
                return RedirectToAction("Login");
            }

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                TempData["ErrorMessage"] = "Invalid user session. Please log in again.";
                return RedirectToAction("Login");
            }

            // Redirect to Events page with user context
            return RedirectToAction("Index", "Event");
        }
    }
}