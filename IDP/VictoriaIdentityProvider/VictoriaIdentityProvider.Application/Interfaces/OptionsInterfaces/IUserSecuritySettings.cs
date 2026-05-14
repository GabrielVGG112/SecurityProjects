using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Application.Interfaces.OptionsInterfaces
{
    /// <summary>
    /// Defines the contract for user security settings, including token expiration, lockout policies, and email
    /// verification requirements.a
    /// </summary>
    /// <remarks>Implementations of this interface provide configuration values that control user
    /// authentication and account security behaviors. These settings are typically used by authentication and user
    /// management components to enforce security policies such as token lifetimes, login attempt limits, and email
    /// verification requirements.</remarks>
 public interface IUserSecuritySettings
        {
            // Token Settings
            int EmailVerificationTokenExpirationHours { get; }
            int PasswordResetTokenExpirationHours { get; }

            // Lockout Settings
            int MaxLoginAttempts { get; }
            int LockoutDurationMinutes { get; }

            // Email Verification
            bool RequireEmailVerification { get; }
        }
    }

