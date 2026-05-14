using VictoriaIdentityProvider.Application.DTOs;

namespace VictoriaIdentityProvider.Application.Services.ClientServices
{
    public interface ISessionService
    {
        Task<TokenResultDto> GenerateNewSessionAsync(LoginDto loginDto);
    }
}