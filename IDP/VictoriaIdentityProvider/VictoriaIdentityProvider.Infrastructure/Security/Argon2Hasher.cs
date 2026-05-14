using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;
using VictoriaIdentityProvider.Infrastructure.Config.OptionsModels;

namespace VictoriaIdentityProvider.Infrastructure.Security
{
    public class Argon2Hasher : IHasher
    {
        private readonly Argon2Options _options;
        public Argon2Hasher(IOptions<Argon2Options> options)
        {
            _options = options.Value;
        }

        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[_options.SaltSize];

            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(salt);
            return salt;
        }


        public string HashData(string password)
        {
            var salt = GenerateSalt();
            var bytesPasswordWithPepper = Encoding.UTF8.GetBytes(password + _options.Pepper);
            using var argon2 = new Argon2id(bytesPasswordWithPepper);
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = _options.Paralelism;
            argon2.Iterations = _options.Iterations;
            argon2.MemorySize = _options.MemorySize;
            byte[] hashBytes = argon2.GetBytes(_options.HashSize);

            return $"{_options.Signature}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hashBytes)}";
        }

        public bool VerifyData(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            if (string.IsNullOrWhiteSpace(hashedPassword)) return false;

            var parts = hashedPassword.Split('$');

         
            if (parts.Length != 6) return false;
            if (!parts[1].Equals("argon2id", StringComparison.OrdinalIgnoreCase)) return false;
            if (!parts[2].StartsWith("v=19")) return false;

            var parameters = parts[3].Split(',');

            if (parameters.Length != 3) return false;
            if (!parameters[0].StartsWith("m=")) return false;
            if (!parameters[1].StartsWith("t=")) return false;
            if (!parameters[2].StartsWith("p=")) return false;

            try
            {
                var memory = int.Parse(parameters[0].Substring(2)); // m=65536
                var iterations = int.Parse(parameters[1].Substring(2)); // t=3
                var parallelism = int.Parse(parameters[2].Substring(2)); // p=1

                var salt = Convert.FromBase64String(parts[4]);
                var hash = Convert.FromBase64String(parts[5]);

                var passwordWithPepper = Encoding.UTF8.GetBytes(password + _options.Pepper);

                using var argon2 = new Argon2id(passwordWithPepper);
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = parallelism;
                argon2.Iterations = iterations;
                argon2.MemorySize = memory;
                var computedHash = argon2.GetBytes(hash.Length);

                // FIX: Use SequenceEqual to compare byte array contents
                return hash.SequenceEqual(computedHash);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
