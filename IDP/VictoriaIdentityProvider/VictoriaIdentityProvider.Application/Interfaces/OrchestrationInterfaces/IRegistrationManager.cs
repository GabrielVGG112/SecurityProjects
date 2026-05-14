using VictoriaIdentityProvider.Application.DTOs;

namespace VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces
{
    public interface IRegistrationManager
    {
        Task RegisterUserAsync(RegisterDto dto);
        Task ResendVerificationLinkAsync(string emailAddress);
        Task ValidateEmailToken(string emailToken);
    }
}
