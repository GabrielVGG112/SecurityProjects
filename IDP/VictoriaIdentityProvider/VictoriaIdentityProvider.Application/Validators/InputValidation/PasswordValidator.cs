using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.Interfaces.ValidatorInterfaces;
using VictoriaIdentityProvider.Application.Services.Factory;

namespace VictoriaIdentityProvider.Application.Validators.InputValidation
{
    public  class PasswordValidator : IAsyncValidator<string>
    {
        private readonly SecurityOptions _options;
        private readonly HttpClient? _httpClient;
       

      


        public PasswordValidator(IOptions<SecurityOptions> options, HttpClient? httpClient)
        {
            _options = options.Value;
            _httpClient = httpClient;
          
        }

        public async Task<ValidationResult> Validate(string password) 
        {
            var errors = new List<string>();

          
            password = password?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(password)) 
                return ValidationResult.Failure("Password is required");

           
            if (password.Length > _options.PasswordMaxLength)
                errors.Add($"Password must not exceed {_options.PasswordMaxLength} characters");


            if (password.Any(c => char.IsControl(c)))
            {
                errors.Add("Password contains invalid characters");
               
            }
        
            if (password.Length < _options.PasswordMinLength) 
                errors.Add($"Password must be at least {_options.PasswordMinLength} characters");

            if (_options.PasswordRequireDigit && !password.Any(char.IsDigit))
                errors.Add("Password must contain at least one digit");

            if (_options.PasswordRequireUppercase && !password.Any(char.IsUpper))
                errors.Add("Password must contain at least one uppercase letter");

            if (_options.PasswordRequireLowercase && !password.Any(char.IsLower))
                errors.Add("Password must contain at least one lowercase letter");

            if (_options.PasswordRequireSpecialChar && !password.Any(c => !char.IsLetterOrDigit(c)))
                errors.Add("Password must contain at least one special character");
            if (_options.CheckBreachedPasswords && _httpClient != null)
            {
                var breachCheck =  await CheckBreachedPassword(password);
                if (!breachCheck.IsValid)
                    errors.AddRange(breachCheck.Errors);
            }

            return errors.Any() ?
                ValidationResult.Failure(errors) :
                ValidationResult.Success();
        }

        /// <summary>
        /// Checks whether the specified password has appeared in known data breaches using a k-anonymity API.
        /// </summary>
        /// <remarks>This method uses a partial hash of the password to query a remote service, preserving
        /// user privacy. If the remote service is unavailable, the method assumes the password is safe and returns a
        /// success result. Callers should consider the potential risk of false negatives if network errors
        /// occur.</remarks>
        /// <param name="password">The password to check for exposure in public data breaches. Cannot be null.</param>
        /// <returns>A ValidationResult indicating whether the password has been found in a data breach. Returns a failure result
        /// if the password is compromised; otherwise, returns a success result.</returns>
        private  async Task<ValidationResult> CheckBreachedPassword(string password)
        {
            try {
                var hash = ComputeSha1Hash(password);
                var prefix = hash[..5];
                var suffix = hash[5..];

                var response = await _httpClient!.GetStringAsync($"range/{prefix}");
                
                if (response.Contains(suffix, StringComparison.OrdinalIgnoreCase)) 
                    // here actually happens the magic
                    return ValidationResult.Failure("This password has been found in a data breach ");
               

                return ValidationResult.Success();

            }
            catch (HttpRequestException) 
            {
                return ValidationResult.Success();
            }
            }


        /// <summary>
        /// Computes the SHA-1 hash of the specified input string and returns its hexadecimal representation.
        /// </summary>
        /// <remarks>The returned hash is always a 40-character uppercase hexadecimal string. SHA-1 is
        /// considered cryptographically weak and should not be used for security-sensitive purposes.</remarks>
        /// <param name="input">The input string to compute the SHA-1 hash for. Cannot be null.</param>
        /// <returns>A string containing the hexadecimal representation of the SHA-1 hash of the input.</returns>
        private  static string ComputeSha1Hash(string input) 
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = SHA1.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
