using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.Interfaces.ValidatorInterfaces;
using ValidationResult = VictoriaIdentityProvider.Application.Services.Factory.ValidationResult;

namespace VictoriaIdentityProvider.Application.Validators.InputValidation;

public class EmailValidator : IValidator<string>
{
    private static readonly HashSet<string> _freeEmailDomains = new()
    {
        "gmail.com", "yahoo.com", "outlook.com", "hotmail.com",
        "icloud.com", "protonmail.com", "aol.com", "mail.com",
        "yahoo.co.uk", "outlook.fr", "gmx.com", "gmx.de"
    };

    private readonly string[] _allowedDomains;
   
    private readonly SecurityOptions _options;


    public EmailValidator(IOptions<SecurityOptions> options)
    {
        _options = options.Value;
        _allowedDomains = ParseAllowedDomains(_options.AllowedEmailDomains);
    }

    public ValidationResult Validate(string emailAddress) 
    {
        var errors = new List<string>();

        // Input sanitization: Handle null and trim whitespace
        emailAddress = emailAddress?.Trim() ?? string.Empty;

     
        if (string.IsNullOrWhiteSpace(emailAddress)) 
            return ValidationResult.Failure("Email is required");


        if (emailAddress.Any(c => char.IsControl(c)))
        {
          
            return ValidationResult.Failure("Email contains invalid characters");
        }
      
        if (!IsValidEmailFormat(emailAddress))
            return ValidationResult.Failure("Invalid email format");

        // Validate maximum length (RFC 5321)
        if (emailAddress.Length > _options.EmailMaxLength)
            errors.Add($"Email must not exceed {_options.EmailMaxLength} characters");

        var domain = GetDomain(emailAddress);

        // Check free email provider policy
        if (!_options.AllowFreeEmailProviders && IsFreeEmailDomain(domain))
            errors.Add("Business emails only. Free email providers are not allowed.");

        // Check domain whitelist
        if (_options.EnforceEmailDomainWhitelist && !IsValidDomain(domain))
            errors.Add($"Only emails from {string.Join(", ", _allowedDomains)} are allowed");

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    private bool IsValidEmailFormat(string emailAddress) 
    {
        var emailAttribute = new EmailAddressAttribute();
        return emailAttribute.IsValid(emailAddress);
    }

    private string GetDomain(string emailAddress) 
    {
        return emailAddress.Split("@").Last().ToLower();
    }

    private string[] ParseAllowedDomains(string domains) 
    {
        if (string.IsNullOrWhiteSpace(domains))
        {
            if (_options.EnforceEmailDomainWhitelist)
                throw new InvalidOperationException(
                    "EnforceEmailDomainWhitelist is enabled but AllowedEmailDomains is not configured");

            return Array.Empty<string>();
        }

        return domains.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(d => d.Trim().ToLower())
                      .ToArray();
    }

    private bool IsValidDomain(string domain) 
    {
        return _allowedDomains.Contains(domain.ToLower());
    }

    private bool IsFreeEmailDomain(string domain) 
    {
        return _freeEmailDomains.Contains(domain.ToLower());
    }


}
