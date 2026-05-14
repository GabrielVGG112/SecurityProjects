using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.Validators.InputValidation;

namespace VictoriaIdentityProvider.Tests.NewTests.Validators;

public class PasswordValidatorTests
{
    private readonly PasswordValidator _validator;
    private readonly SecurityOptions _defaultOptions;

    public PasswordValidatorTests()
    {
        _defaultOptions = new SecurityOptions
        {
            PasswordMinLength = 8,
            PasswordMaxLength = 128,
            PasswordRequireDigit = true,
            PasswordRequireUppercase = true,
            PasswordRequireLowercase = true,
            PasswordRequireSpecialChar = true
        };

        _validator = new PasswordValidator(Options.Create(_defaultOptions), null);
    }

    [Fact]
    public async Task Validate_WithValidPassword_ShouldReturnSuccess()
    {
  
        var password = "SecureP@ssw0rd";

     
        var result =  await _validator.Validate(password);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_ShouldReturnFailure()
    {

        var password = "";

   
        var result = await _validator.Validate(password);

        Assert.False(result.IsValid);
        Assert.Contains("Password is required", result.Errors);
    }

    [Fact]
    public async Task Validate_WithNullPassword_ShouldReturnFailure()
    {

        string password = null!;

    
        var result = await _validator.Validate(password);

  
        Assert.False(result.IsValid);
        Assert.Contains("Password is required", result.Errors);
    }

    [Fact]
    public async Task Validate_WithPasswordTooShort_ShouldReturnFailure()
    {
     
        var password = "Short1!";

        var result = await _validator.Validate(password);

     
        Assert.False(result.IsValid);
        Assert.Contains($"Password must be at least {_defaultOptions.PasswordMinLength}", result.Errors.First());
    }

    [Fact]
    public async Task Validate_WithPasswordMissingDigit_ShouldReturnFailure()
    {
      
        var password = "NoDigitPassword!";

      
        var result = await _validator.Validate(password);

       
        Assert.False(result.IsValid);
        Assert.Contains("Password must contain at least one digit", result.Errors);
    }

    [Fact]
    public async Task Validate_WithPasswordMissingUppercase_ShouldReturnFailure()
    {
    
        var password = "nouppercase123!";

      
        var result =await _validator.Validate(password);

       
        Assert.False(result.IsValid);
        Assert.Contains("Password must contain at least one uppercase letter", result.Errors);
    }

    [Fact]
    public async Task Validate_WithPasswordMissingLowercase_ShouldReturnFailure()
    {
       
        var password = "NOLOWERCASE123!";

      
        var result = await _validator.Validate(password);

      
        Assert.False(result.IsValid);
        Assert.Contains("Password must contain at least one lowercase letter", result.Errors);
    }

    [Fact]
    public async Task Validate_WithPasswordMissingSpecialChar_ShouldReturnFailure()
    {
      
        var password = "NoSpecialChar123";

     
        var result = await _validator.Validate(password);

        
        Assert.False(result.IsValid);
        Assert.Contains("Password must contain at least one special character", result.Errors);
    }

    [Fact]
    public async Task Validate_WithMultipleViolations_ShouldReturnAllErrors()
    {
       
        var password = "weak";

       
        var result =await _validator.Validate(password);

       
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count > 1);
    }

    [Fact]
    public async Task Validate_WithMinimumRequirements_ShouldReturnSuccess()
    {
        
        var password = "Passw0rd!";

      
        var result =await _validator.Validate(password);

   
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Validate_WithSpecialCharactersOnly_ShouldRequireOtherRules()
    {
        
        var password = "!@#$%^&*";

      
        var result = await _validator.Validate(password);

      
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("digit"));
        Assert.Contains(result.Errors, e => e.Contains("uppercase"));
        Assert.Contains(result.Errors, e => e.Contains("lowercase"));
    }

    [Fact]
    public async Task Validate_WithCustomOptions_NoDigitRequired_ShouldAcceptPasswordWithoutDigit()
    {
      
        var options = new SecurityOptions
        {
            PasswordMinLength = 8,
            PasswordRequireDigit = false,
            PasswordRequireUppercase = true,
            PasswordRequireLowercase = true,
            PasswordRequireSpecialChar = true
        };
        var validator = new PasswordValidator(Options.Create(options), null);
        var password = "NoDigitPass!";

     
        var result = await validator.Validate(password);

       
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithVeryLongPassword_ShouldReturnSuccess()
    {
    
        var password = "VeryLongP@ssw0rd" + new string('x', 100);

       
        var result =await _validator.Validate(password);

     
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithPasswordContainingControlCharacters_ShouldReturnFailure()
    {
      
        var password = "Pass\u0001word123!";

       
        var result = await _validator.Validate(password);

     
        Assert.False(result.IsValid);
        Assert.Contains("Password contains invalid characters", result.Errors);
    }

    [Fact]
    public async Task Validate_WithPasswordContainingControlCharacters_ShouldLogCritical()
    {
      
        var mockLogger = new Mock<ILogger<PasswordValidator>>();
        var validator = new PasswordValidator(Options.Create(_defaultOptions), null);
        var password = "Pass\u0001word123!";

      
        var result = await validator.Validate(password);

      
        Assert.False(result.IsValid);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attemt to use Control Characters")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Validate_WithPasswordExceedingMaxLength_ShouldReturnFailure()
    {
     
        var password = "ValidP@ss1" + new string('x', _defaultOptions.PasswordMaxLength);

    
        var result = await _validator.Validate(password);

        Assert.False(result.IsValid);
        Assert.Contains($"Password must not exceed {_defaultOptions.PasswordMaxLength} characters", result.Errors);
    }

    [Fact]
    public async Task Validate_WithBreachedPasswordCheckEnabled_WhenPasswordIsBreached_ShouldReturnFailure()
    {
 
        var options = new SecurityOptions
        {
            PasswordMinLength = 8,
            PasswordMaxLength = 128,
            PasswordRequireDigit = false,
            PasswordRequireUppercase = false,
            PasswordRequireLowercase = false,
            PasswordRequireSpecialChar = false,
            CheckBreachedPasswords = true
        };
        var httpClient = new HttpClient(new MockHttpMessageHandler(true))
        {
            BaseAddress = new Uri("https://api.pwnedpasswords.com/")
        };
        var validator = new PasswordValidator(Options.Create(options), httpClient);
        var password = "password123";

    
        var result =await validator.Validate(password);

           Assert.False(result.IsValid);
        Assert.Contains("This password has been found in a data breach ", result.Errors);
    }

    [Fact]
    public async Task Validate_WithBreachedPasswordCheckEnabled_WhenPasswordIsNotBreached_ShouldReturnSuccess()
    {
       
        var options = new SecurityOptions
        {
            PasswordMinLength = 8,
            PasswordMaxLength = 128,
            PasswordRequireDigit = false,
            PasswordRequireUppercase = false,
            PasswordRequireLowercase = false,
            PasswordRequireSpecialChar = false,
            CheckBreachedPasswords = true
        };
        var httpClient = new HttpClient(new MockHttpMessageHandler(false))
        {
            BaseAddress = new Uri("https://api.pwnedpasswords.com/")
        };
        var validator = new PasswordValidator(Options.Create(options), httpClient);
        var password = "password123";

       
        var result = await validator.Validate(password);

     
        Assert.True(result.IsValid);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly bool _simulateBreach;

        public MockHttpMessageHandler(bool simulateBreach)
        {
            _simulateBreach = simulateBreach;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
         
            var prefix = request.RequestUri?.Segments.Last() ?? string.Empty;
            
            var hash = System.Security.Cryptography.SHA1.HashData(System.Text.Encoding.UTF8.GetBytes("password123"));
            var hashString = Convert.ToHexString(hash);
            var suffix = hashString[5..];
            
         
            string response;
            if (_simulateBreach)
            {
             
                response = $"{suffix}:1000\r\nOTHERHASH:500";
            }
            else
            {
                
                response = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA:1000\r\nBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB:500";
            }

            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(response)
            });
        }
    }
}
