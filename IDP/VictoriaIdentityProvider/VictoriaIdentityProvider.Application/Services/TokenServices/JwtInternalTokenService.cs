using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;

namespace VictoriaIdentityProvider.Application.Services.TokenServices;

public class JwtInternalTokenService : IJwtInternalToken
{
    private readonly JwtKeysOptions _tokenKeys;
   
   

   

    public JwtInternalTokenService(
        IOptions<JwtKeysOptions> options
       )
    {
        _tokenKeys = options.Value;
   
        
    }
    public string GenerateJwtInternalToken(string purpose, Guid userId, int hours)
    {
        var securityKey = new SymmetricSecurityKey(WebEncoders.Base64UrlDecode(_tokenKeys.UserActionTokenKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);


        var token = new JwtSecurityToken(
            issuer: "VictoriaIDP",
            audience: "VictoriaAPI",
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub,userId.ToString()),
                new Claim(TokenPurposeSettings.ForPurpose,purpose),

                ],
            expires: DateTime.UtcNow.AddHours(hours),
            signingCredentials: credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Guid? ValidateJwtInternalToken(string token, string expectedPurpose)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(WebEncoders.Base64UrlDecode(_tokenKeys.UserActionTokenKey));


            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "VictoriaIDP",
                ValidAudience = "VictoriaAPI",
                IssuerSigningKey = securityKey
            }, out _);
            var purpose = principal.FindFirst(TokenPurposeSettings.ForPurpose);

            if (purpose is null || purpose.Value != expectedPurpose)
                return null;

            var sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value; //remaps silently ...
            if (!Guid.TryParse(sub, out Guid userId)) return null;

            return userId;
        }
        catch (SecurityTokenExpiredException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }

}
