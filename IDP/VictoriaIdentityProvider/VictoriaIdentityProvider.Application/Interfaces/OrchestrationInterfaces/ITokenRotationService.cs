using VictoriaIdentityProvider.Application.DTOs;

namespace VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces
{
    public interface ITokenRotationService
    {
        Task<TokenResultDto> RotateTokensAsync(TokenResultDto loginResult);
    }
}