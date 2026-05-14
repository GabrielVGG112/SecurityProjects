using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.EventBus.Interfaces;
using VictoriaIdentityProvider.Application.Events;
using VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces;
using VictoriaIdentityProvider.Domain.CustomErrors;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Services.ClientServices;


public class LoginService : ILoginService
{
    
    private readonly IUserService _userService;


 
    public LoginService(IUserService userService)
    {
        _userService = userService;

    }

    public async Task<User> LoginUserAsync(LoginDto dto) 
    {
        var user =await _userService.GetUserByEmailAddressAsync(dto.Email);

        _userService.ValidateEmailConfirmation(user); // throws EmailConfirmationException();
        _userService.ValidateLockout(user); // throws LockoutExpirationException();

        try
        {
            await _userService.ValidatePassword(user, dto.Password); 
        } 
        catch (ValidationError e)
        {

            await _userService.MakeUserLockout(user);
            throw;
          
        }
        user.LastLogInAt = DateTime.UtcNow;
        user.UserStatus = UserStatusEnum.Active;
       
       

        await _userService.UpdateUserAsync(user);
        return user;
    }

}
