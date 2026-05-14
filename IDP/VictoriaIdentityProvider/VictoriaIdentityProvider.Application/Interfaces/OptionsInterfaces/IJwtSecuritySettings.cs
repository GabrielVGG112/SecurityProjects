using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Application.Interfaces.OptionsInterfaces
{
    public interface IJwtSecuritySettings
    {
        public int EmailVerificationTokenExpirationHour { get; }
        public int PasswordResetTokenExpirationHours { get; }
        public int JwtTokenExpiresInMinutes  { get; }
        public int TokenExpirationInDays  { get; }
    }
}
