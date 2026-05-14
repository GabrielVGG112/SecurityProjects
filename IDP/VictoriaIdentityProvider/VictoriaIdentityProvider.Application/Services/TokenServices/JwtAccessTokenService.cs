using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.Interfaces.OptionsInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;

namespace VictoriaIdentityProvider.Application.Services.TokenServices
{
    public class JwtAccessTokenService : IJwtAccessTokenService
    {
        private readonly JwtKeysOptions _tokenKeys;
        private readonly IJwtSecuritySettings _securitySettings;
        private readonly byte[] _victoriaIdpClientSecret;
        private readonly IHasher _hasher;
        public JwtAccessTokenService
            (
            IOptions<JwtKeysOptions> options,
            IJwtSecuritySettings securitySettings,
           [FromKeyedServices("hmac")] IHasher hasher
            )
        {
            _tokenKeys = options.Value;
            _securitySettings = securitySettings;
            _victoriaIdpClientSecret = Encoding.UTF8.GetBytes(_tokenKeys.VictoriaIdpClientSecret);
            _hasher = hasher;
        }


        public string GenerateJwtSymmetricToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(_victoriaIdpClientSecret);
            var signInCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenOptions = new JwtSecurityToken(
                issuer: "VictoriaIDP",
                audience: "VictoriaAPI",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_securitySettings.JwtTokenExpiresInMinutes),
                signingCredentials: signInCredentials);



            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return token;
        }

        public ClaimsPrincipal? ValidateJwtSymmetricToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(_victoriaIdpClientSecret);
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "VictoriaIDP",
                    ValidAudience = "VictoriaAPI",
                    IssuerSigningKey = securityKey
                }, out _);
                return principal;

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
}

