using EventManagementApp.DTOs;
using EventManagementApp.Models.Entities;

public interface IUserService
{
    Task<User> RegisterAsync(RegisterDto registerDto);
    Task<User> LoginAsync(LoginDto loginDto);
    Task<User> GetByIdAsync(Guid id);
    Task<User> GetByUsernameAsync(string username);
}