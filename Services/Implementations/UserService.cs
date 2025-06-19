using EventManagementApp.DTOs;
using EventManagementApp.Models.Entities;
using EventManagementApp.Models.Enum;
using EventManagementApp.Repositories.Interfaces;
using EventManagementApp.Services.Interfaces;
using System.Security.Cryptography;

namespace EventManagementApp.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> RegisterAsync(RegisterDto registerDto)
        {
            // Validate password confirmation
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                throw new InvalidOperationException("Password and confirmation password do not match.");
            }

            // Check if user already exists
            if (await _userRepository.ExistsByUsernameOrEmailAsync(registerDto.Username, registerDto.Email))
            {
                throw new InvalidOperationException("User with this username or email already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                Role = Role.Customer
            };

            return await _userRepository.AddAsync(user);
        }

        public async Task<User> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            return user;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "EventApp_Salt"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}