using EventManagementApp.Models.Entities;

namespace EventManagementApp.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User user);
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetByUsernameAsync(string username);
        Task<bool> ExistsByUsernameOrEmailAsync(string username, string email);
    }
}
