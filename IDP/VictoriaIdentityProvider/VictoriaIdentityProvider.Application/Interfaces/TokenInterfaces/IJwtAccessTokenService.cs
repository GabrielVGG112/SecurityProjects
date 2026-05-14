using System.Security.Claims;

namespace VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;

public interface IJwtAccessTokenService
{
    string GenerateJwtSymmetricToken(IEnumerable<Claim> claims);

    ClaimsPrincipal? ValidateJwtSymmetricToken(string token);


}