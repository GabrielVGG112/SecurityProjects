using VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Services.Factory
{
    public class SessionFactory : IModelFactory<UserSession>
    {
        private readonly IDefaultTokenService _defaultTokenService;
        private readonly IClientContextService _context;
        public SessionFactory(IDefaultTokenService defaultTokenService, IClientContextService context)
        {
            _defaultTokenService = defaultTokenService;
            _context = context;
        }

        public (UserSession, string) CreateModel(User user)
        {
            var metadata = _context.GetClientMetadata();

            var (rawToken, expiration) = _defaultTokenService.GenerateRandomToken();

            UserSession session = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RefreshTokenId = null,
                SessionToken = _defaultTokenService.GetHashFromToken(rawToken),
                DeviceId = metadata.DeviceId,
                DeviceName = metadata.DeviceName,
                IpAddress = metadata.IpAddress,
                UserAgent = metadata.UserAgent,
                Location = metadata.Location, // it will be for now unknown i need to integrate MaxMind and extend the model
                ExpiresAt = DateTime.UtcNow.AddDays(expiration),
                RevokedAt = null,
                RevokedReason = null,
                // should be add first after the refresh token on to many 
                // refreshtokenid is for active token id only one active per session 
            };

            return (session, rawToken);
        }
    }
}
