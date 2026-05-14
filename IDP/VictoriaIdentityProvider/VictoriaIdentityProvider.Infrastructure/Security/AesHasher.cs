using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;

namespace VictoriaIdentityProvider.Infrastructure.Security
{
    public  class AesHasher :ICipher
    {

        private readonly byte[] _key;
        public AesHasher(IConfiguration config)
        {
            string key = config["Security:Aes:Key"] ?? string.Empty;

            if(string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException("Missing Aes Key");
            _key = Convert.FromBase64String(key);
        }

        public string Encrypt(string plainText) 
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            using var aes = Aes.Create();

            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var hmac = new HMACSHA256(_key);
            aes.IV = hmac.ComputeHash(Encoding.UTF8.GetBytes(plainText))[..16];


            using var encryptor =aes.CreateEncryptor();
            var plainTextbytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainTextbytes, 0, plainTextbytes.Length);

            return $"{Convert.ToBase64String(aes.IV)}:{Convert.ToBase64String(cipherBytes)}";
        }

        public string  Decrypt(string cipherText) 
        {
            if (string.IsNullOrWhiteSpace(cipherText)) return cipherText;

            var parts = cipherText.Split(':');

            if (parts.Length != 2) 
                throw new FormatException("Invalid Encryption Format");

            var iv = Convert.FromBase64String(parts[0]);
            var cipherbytes = Convert.FromBase64String(parts[1]);


            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;


            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherbytes, 0, cipherbytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
