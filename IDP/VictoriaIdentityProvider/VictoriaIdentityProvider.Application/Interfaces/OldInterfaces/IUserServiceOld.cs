using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Interfaces.OldInterfaces;

/// <summary>
/// Defines the contract for user-related application services.
/// </summary>
[Obsolete]
public interface IUserServiceOld
{
    Task<User> LogInUserAsync(LoginDto dto);
    Task<User> RegisterUserAsync(RegisterDto user);
    Task ResetPasswordAsync(string passwordToken, ResetPasswordDto resetPassword);
    Task ForgotPasswordAsync(string email);
    Task ValidateEmailToken(string emailToken);
}
