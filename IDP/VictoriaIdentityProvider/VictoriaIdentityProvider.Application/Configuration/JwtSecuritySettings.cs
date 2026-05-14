using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Application.Interfaces.OptionsInterfaces;

namespace VictoriaIdentityProvider.Application.Configuration
{
    public class JwtSecuritySettings :IJwtSecuritySettings
    {
        private readonly SecurityOptions _options;

        public JwtSecuritySettings(IOptions<SecurityOptions> options)
        {
            _options = options.Value;

        }

        public int EmailVerificationTokenExpirationHour => _options.EmailVerificationTokenExpirationHours;
        public int PasswordResetTokenExpirationHours =>_options.PasswordResetTokenExpirationHours;
        public int JwtTokenExpiresInMinutes => _options.JwtTokenExpiresInMinutes;
        public int TokenExpirationInDays  => _options.TokenExpirationInDays;
    }
}
