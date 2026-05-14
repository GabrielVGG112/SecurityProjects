namespace VictoriaIdentityProvider.Application.Configuration;

/// <summary>
/// Application-level security policies and validation rules
/// </summary>
public class SecurityOptions
{

    // Password 
    public int PasswordMinLength { get; set; } = 8;
    public int PasswordMaxLength { get; set; } = 128;
    public bool PasswordRequireDigit { get; set; } = true;
    public bool PasswordRequireUppercase { get; set; } = true;
    public bool PasswordRequireLowercase { get; set; } = true;
    public bool PasswordRequireSpecialChar { get; set; } = true;
    public bool CheckBreachedPasswords { get; set; }
    // Email Validation 
    public bool RequireEmailVerification { get; set; } = true;
    public int EmailMaxLength { get; set; } = 254; // RFC 5321 standard
    public bool AllowFreeEmailProviders { get; set; } = true; // Allow Gmail, Outlook, Yahoo (needed for OAuth social login)
    public bool EnforceEmailDomainWhitelist { get; set; } = false; // Only allowed domains
    public string AllowedEmailDomains { get; set; } = string.Empty; // Comma-separated: "company.com,partner.com"

    public int MaxLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 30;

    // Token Expiration
    public int EmailVerificationTokenExpirationHours { get; set; } = 24;
    public int PasswordResetTokenExpirationHours { get; set; } = 1;
    public int JwtTokenExpiresInMinutes { get; set; } = 15;
    public int TokenExpirationInDays { get; set; } = 15;

   
}
