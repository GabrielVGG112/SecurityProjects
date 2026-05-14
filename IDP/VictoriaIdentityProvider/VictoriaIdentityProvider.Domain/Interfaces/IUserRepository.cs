using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task<bool> VerifyIfEmailExistsAsync(string email);
        Task<bool> VerifyIfIdExistsAsync(Guid id);
        Task UpdateUserAsync(User user);
        Task<User> GetUserWithNavigationsById(Guid id);


    }
}
