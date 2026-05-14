using Microsoft.Extensions.Options;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.Validators.InputValidation;

namespace VictoriaIdentityProvider.Tests.NewTests.Validators;

public class EmailValidatorTests
{
    private readonly EmailValidator _validator;
    private readonly SecurityOptions _defaultOptions;

    public EmailValidatorTests()
    {
        _defaultOptions = new SecurityOptions
        {
            RequireEmailVerification = true,
            EmailMaxLength = 254,
            AllowFreeEmailProviders = true,
            EnforceEmailDomainWhitelist = false,
            AllowedEmailDomains = string.Empty
        };

        _validator = new EmailValidator(Options.Create(_defaultOptions));
    }

    [Fact]
    public void Validate_WithValidEmail_ShouldReturnSuccess()
    {
      
        var email = "user@example.com";

      
        var result = _validator.Validate(email);

       
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldReturnFailure()
    {
      
        var email = "";

      
        var result = _validator.Validate(email);

       
        Assert.False(result.IsValid);
        Assert.Contains("Email is required", result.Errors);
    }

    [Fact]
    public void Validate_WithNullEmail_ShouldReturnFailure()
    {
      
        string email = null!;

      
        var result = _validator.Validate(email);

      
        Assert.False(result.IsValid);
        Assert.Contains("Email is required", result.Errors);
    }

    [Fact]
    public void Validate_WithInvalidEmailFormat_ShouldReturnFailure()
    {
       
        var email = "notanemail";

       
        var result = _validator.Validate(email);

       
        Assert.False(result.IsValid);
        Assert.Contains("Invalid email format", result.Errors);
    }

    [Fact]
    public void Validate_WithEmailTooLong_ShouldReturnFailure()
    { 
        var email = new string('a', 250) + "@test.com";

       
        var result = _validator.Validate(email);

       
        Assert.False(result.IsValid);
        Assert.Contains("Email must not exceed", result.Errors.First());
    }

    [Fact]
    public void Validate_WithGmailAddress_AllowFreeProviders_ShouldReturnSuccess()
    {
       
        var email = "user@gmail.com";

       
        var result = _validator.Validate(email);

       
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithGmailAddress_BlockFreeProviders_ShouldReturnFailure()
    {
       
        var options = new SecurityOptions
        {
            AllowFreeEmailProviders = false,
            EnforceEmailDomainWhitelist = false,
            EmailMaxLength = 254
        };
        var validator = new EmailValidator(Options.Create(options));
        var email = "user@gmail.com";

       
        var result = validator.Validate(email);

        
        Assert.False(result.IsValid);
        Assert.Contains("Business emails only. Free email providers are not allowed.", result.Errors.First());
    }

    [Theory]
    [InlineData("user@yahoo.com")]
    [InlineData("user@outlook.com")]
    [InlineData("user@hotmail.com")]
    [InlineData("user@icloud.com")]
    [InlineData("user@protonmail.com")]
    public void Validate_WithFreeEmailProviders_BlockEnabled_ShouldReturnFailure(string email)
    {
       
        var options = new SecurityOptions
        {
            AllowFreeEmailProviders = false,
            EnforceEmailDomainWhitelist = false,
            EmailMaxLength = 254
        };
        var validator = new EmailValidator(Options.Create(options));

        
        var result = validator.Validate(email);

       
        Assert.False(result.IsValid);
        Assert.Contains("Business emails only. Free email providers are not allowed.", result.Errors.First());
    }

    [Fact]
    public void Validate_WithWhitelistEnabled_AllowedDomain_ShouldReturnSuccess()
    {
        // Arrange
        var options = new SecurityOptions
        {
            EnforceEmailDomainWhitelist = true,
            AllowedEmailDomains = "company.com,partner.org",
            AllowFreeEmailProviders = true,
            EmailMaxLength = 254
        };
        var validator = new EmailValidator(Options.Create(options));
        var email = "user@company.com";

      
        var result = validator.Validate(email);

       
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithWhitelistEnabled_DisallowedDomain_ShouldReturnFailure()
    {
        
        var options = new SecurityOptions
        {
            EnforceEmailDomainWhitelist = true,
            AllowedEmailDomains = "company.com,partner.org",
            AllowFreeEmailProviders = true,
            EmailMaxLength = 254
        };
        var validator = new EmailValidator(Options.Create(options));
        var email = "user@notallowed.com";

      
        var result = validator.Validate(email);

      
        Assert.False(result.IsValid);
        Assert.Contains("Only emails from", result.Errors.First());
    }

    [Fact]
    public void Validate_WithWhitelistEnabled_CaseInsensitive_ShouldReturnSuccess()
    {
       
        var options = new SecurityOptions
        {
            EnforceEmailDomainWhitelist = true,
            AllowedEmailDomains = "Company.COM",
            AllowFreeEmailProviders = true,
            EmailMaxLength = 254
        };
        var validator = new EmailValidator(Options.Create(options));
        var email = "user@company.com";

        
        var result = validator.Validate(email);

      
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithMultipleDomains_ShouldHandleWhitespaces()
    {
      
        var options = new SecurityOptions
        {
            EnforceEmailDomainWhitelist = true,
            AllowedEmailDomains = "  company.com  ,  partner.org  ",
            AllowFreeEmailProviders = true,
            EmailMaxLength = 254
        };
        var validator = new EmailValidator(Options.Create(options));
        var email = "user@partner.org";

       
        var result = validator.Validate(email);

     
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("user+tag@example.com")]
    [InlineData("user.name@example.com")]
    [InlineData("user_name@example.co.uk")]
    [InlineData("first.last@subdomain.example.com")]
    public void Validate_WithValidEmailFormats_ShouldReturnSuccess(string email)
    {
       
        var result = _validator.Validate(email);

       
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("user@")]
    [InlineData("@example.com")]
    [InlineData("user@@example.com")]
    public void Validate_WithInvalidEmailFormats_ShouldReturnFailure(string email)
    {
        
        var result = _validator.Validate(email);

       
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Constructor_WithWhitelistEnabledButNoDomains_ShouldThrowException()
    {
     
        var options = new SecurityOptions
        {
            EnforceEmailDomainWhitelist = true,
            AllowedEmailDomains = string.Empty,
            EmailMaxLength = 254
        };

        
        Assert.Throws<InvalidOperationException>(() => new EmailValidator(Options.Create(options)));
    }

    [Fact]
    public void Validate_WithBusinessEmail_BlockFreeProviders_ShouldReturnSuccess()
    {
       
        var options = new SecurityOptions
        {
            AllowFreeEmailProviders = false,
            EnforceEmailDomainWhitelist = false,
            EmailMaxLength = 254
        };
        var validator = new EmailValidator(Options.Create(options));
        var email = "user@mycompany.com";

       
        var result = validator.Validate(email);

       
        Assert.True(result.IsValid);
    }
}
