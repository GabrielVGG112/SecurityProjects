using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces
{
    public interface IUserService
    {
        Task<User> GetUserByEmailAddressAsync(string emailAddress);
        Task<User> CreateUserAsync(RegisterDto registerDto);
        Task<User> GetUserByIdAsync(Guid id);
        Task UpdateUserAsync(User user);
        Task MakeUserLockout(User user);
        Task ValidatePassword(User user, string password);
        void ValidateLockout(User user);
        void ValidateEmailConfirmation(User user);
       


    }
}
