using VictoriaIdentityProvider.Application.DTOs;

namespace VictoriaIdentityProvider.Application.Interfaces
{
    public interface ILogoutService
    {
        Task LogoutFromAllDevicesAsync(TokenResultDto dto);
        Task LogoutUserAsync(TokenResultDto dto);
    }
}