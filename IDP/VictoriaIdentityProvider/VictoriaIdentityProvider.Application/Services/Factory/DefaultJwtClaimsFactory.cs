using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Services.Factory
{
    public static class DefaultJwtClaimsFactory
    {
        public static Claim[] BuildClaimsForUser(User user, Guid sessionId)
        {
            var now = DateTimeOffset.UtcNow;

            var claims = new List<Claim>
{

        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        

        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.GivenName, user.FirstName),
        new(ClaimTypes.Surname, user.LastName),
        new(ClaimTypes.Name, user.FullName),
    
        new(CustomClaimTypes.EmailVerified,
        user.IsEmailConfirmed ?
        "true" : "false"),
        new(CustomClaimTypes.PreferredUsername, user.Email),
        new(CustomClaimTypes.SessionId, sessionId.ToString()),
};

            return [.. claims];
        }
    }
}
