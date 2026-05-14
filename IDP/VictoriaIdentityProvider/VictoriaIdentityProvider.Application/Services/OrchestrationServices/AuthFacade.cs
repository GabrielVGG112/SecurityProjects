using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Interfaces;
using VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces;
using VictoriaIdentityProvider.Application.Services.ClientServices;

namespace VictoriaIdentityProvider.Application.Services.OrchestrationServices
{
    public class AuthFacade
    {
        private readonly ISessionService _sessionService;
        private readonly ILogoutService _logoutService;
        private readonly ITokenRotationService _tokenRotationService;

        public AuthFacade(
            ISessionService sessionService,
            ILogoutService logoutService,
            ITokenRotationService tokenRotationService)
        {
            _sessionService = sessionService;
            _logoutService = logoutService;
            _tokenRotationService = tokenRotationService;
        }

        public Task<TokenResultDto> LoginAsync(LoginDto dto)
            => _sessionService.GenerateNewSessionAsync(dto);

        public Task LogoutAsync(TokenResultDto dto)
            => _logoutService.LogoutUserAsync(dto);

        public Task LogoutAllDevicesAsync(TokenResultDto dto)
            => _logoutService.LogoutFromAllDevicesAsync(dto);

        public Task<TokenResultDto> RotateTokensAsync(TokenResultDto dto)
            => _tokenRotationService.RotateTokensAsync(dto);
    }
}
