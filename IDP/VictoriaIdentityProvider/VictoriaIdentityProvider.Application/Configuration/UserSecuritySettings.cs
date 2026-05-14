using Microsoft.Extensions.Options;
using VictoriaIdentityProvider.Application.Interfaces.OptionsInterfaces;

namespace VictoriaIdentityProvider.Application.Configuration
{
    /// <summary>
    /// Provides access to user security configuration settings such as token expiration, login attempt limits, and
    /// email verification requirements.
    /// </summary>
    /// <remarks>This class exposes security-related settings for user authentication and account management.
    /// The values are typically configured via application configuration and are intended to be used by authentication
    /// and user management components to enforce security policies.</remarks>
    public class UserSecuritySettings : IUserSecuritySettings
    {
        private readonly SecurityOptions _options;

        public UserSecuritySettings(IOptions<SecurityOptions> options)
        {
            _options = options.Value;
        }

        public int EmailVerificationTokenExpirationHours => _options.EmailVerificationTokenExpirationHours;
        public int PasswordResetTokenExpirationHours => _options.PasswordResetTokenExpirationHours;
        public int MaxLoginAttempts => _options.MaxLoginAttempts;
        public int LockoutDurationMinutes => _options.LockoutDurationMinutes;
        public bool RequireEmailVerification => _options.RequireEmailVerification;
    }
}
