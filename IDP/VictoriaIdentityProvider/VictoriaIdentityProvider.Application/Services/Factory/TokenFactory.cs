using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Services.Factory
{
    public class TokenFactory : ITokenFactory
    {
        IDefaultTokenService _defaultedTokenService;
        IJwtAccessTokenService _jwtAccessTokenService;
 
        public TokenFactory(IDefaultTokenService defaultedTokenService,IJwtAccessTokenService jwtAccessTokenService)
        {
            _defaultedTokenService = defaultedTokenService;
           _jwtAccessTokenService = jwtAccessTokenService;
        }

        public (RefreshToken ,string)CreateModel(User user)
        {
               var (rawToken , expiration) = _defaultedTokenService.GenerateRandomToken();

            RefreshToken token = new RefreshToken()
            {
                Id = Guid.NewGuid(),
                TokenHash = _defaultedTokenService.GetHashFromToken(rawToken),
                ExpiresAt = DateTime.UtcNow.AddDays(expiration),
                UserId = user.Id,
                RevokedAt = null,
                RevokedReason = null,
                ReplacedByTokenId = null,
            };
            return (token, rawToken);
        }

        public string GenerateJwtToken(User user, Guid sessionId)
        {
            var claims = DefaultJwtClaimsFactory.BuildClaimsForUser(user, sessionId);

          return _jwtAccessTokenService.GenerateJwtSymmetricToken(claims);
        }

      
    }
}
