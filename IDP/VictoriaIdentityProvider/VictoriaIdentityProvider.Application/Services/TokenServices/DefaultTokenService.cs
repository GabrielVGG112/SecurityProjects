using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using VictoriaIdentityProvider.Application.Interfaces.OptionsInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;
using VictoriaIdentityProvider.Application.Services.OrchestrationServices;

namespace VictoriaIdentityProvider.Application.Services.TokenServices
{
    public class DefaultTokenService : IDefaultTokenService
    {
        private readonly IJwtSecuritySettings _securitySettings;
        private readonly IHasher _hasher;


        public DefaultTokenService
            (
            IJwtSecuritySettings securitySettiongs,
            HeaderValidatorFacade asyncValidator,
            [FromKeyedServices("hmac")] IHasher hasher
            )
        {
            _securitySettings = securitySettiongs;
            _hasher = hasher;
          
        }

        public string GetHashFromToken(string token)
        {

            return _hasher.HashData(token);
        }
        public (string, int expirationDays) GenerateRandomToken()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            var token = Convert.ToBase64String(bytes);

            return (token, _securitySettings.TokenExpirationInDays);
        }
    }
}
