using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;

namespace VictoriaIdentityProvider.Infrastructure.Security
{
    public class HmacSha256Hasher : IHasher
    {
        private readonly byte[] _key;
        public HmacSha256Hasher(IConfiguration config)
        {
            var keyBase64 = config["Security:HmacSha256:Key"];
            if (string.IsNullOrWhiteSpace(keyBase64))
                throw new InvalidOperationException("Missing HMAC KEY");

            _key = Convert.FromBase64String(keyBase64);
        }
        public string HashData(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentNullException("A token is required");
            if (token.Length != 44) throw new InvalidOperationException("Invalid token");


            using var hmac = new HMACSHA256(_key);
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var hashedBytes = hmac.ComputeHash(tokenBytes);

            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyData(string userToken, string hashedDbToken)
        {
            
            if (string.IsNullOrWhiteSpace(userToken) || string.IsNullOrWhiteSpace(hashedDbToken)) 
                return false;
            
            if (userToken.Length != 44) 
                return false;
            using var hmac = new HMACSHA256(_key);

            var userTokenBytes = Encoding.UTF8.GetBytes(userToken);
            var computeHash = hmac.ComputeHash(userTokenBytes);
            var storedHash = Convert.FromBase64String(hashedDbToken);


            return CryptographicOperations.FixedTimeEquals(computeHash, storedHash);
        }
    }
}
