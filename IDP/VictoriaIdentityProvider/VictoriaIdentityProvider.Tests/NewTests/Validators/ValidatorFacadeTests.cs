using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;
using VictoriaIdentityProvider.Application.Validators;
using VictoriaIdentityProvider.Application.Configuration;
using Microsoft.Extensions.Options;
using VictoriaIdentityProvider.Application.Enums;
using VictoriaIdentityProvider.Application.DTOs;
using Moq;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Application.Services.OrchestrationServices;
using VictoriaIdentityProvider.Application.Validators.InputValidation;


namespace VictoriaIdentityProvider.Tests.NewTests.Validators
{
    public class ValidatorFacadeTests 
    {
        private readonly FieldValidatorFacade _facade;

        public ValidatorFacadeTests()
        {
            var securityOptions = new SecurityOptions
            {
                PasswordRequireDigit = true,
                PasswordRequireLowercase = true,
                PasswordRequireUppercase = true,
                CheckBreachedPasswords = false,
                PasswordMinLength = 8,
                PasswordMaxLength = 128,
                MaxLoginAttempts = 3,
                RequireEmailVerification = false,
                LockoutDurationMinutes = 15,
                EmailVerificationTokenExpirationHours = 24,
                PasswordResetTokenExpirationHours = 24,
            };
           
            var options = Options.Create(securityOptions);
            var emailValidator = new EmailValidator(options);
            var  passwordValidator = new PasswordValidator(options, null);
            var phoneNumberValidator = new PhoneNumberValidator();
            var nameValidator = new NameValidator();

            _facade = new FieldValidatorFacade(emailValidator, passwordValidator,nameValidator, phoneNumberValidator);
        }

        [Fact]
        public void ValidateField_WithValidEmail_ShouldReturnSuccess() 
        {

            string email = "test@example.com";

            var result = _facade.ValidateRegistration(ValidationField.Email, email);

            Assert.True(result.IsValid);

        }
        [Fact]

        public void ValidateField_WithNullValue_ShouldReturnFailure() 
        {
            string? email = null;
            var result = _facade.ValidateRegistration(ValidationField.Email, email);

            Assert.False(result.IsValid);
        }
        [Fact]

        public void ValidateField_WithControlChar_ShouldReturnFailure()
        {
            string email = "test@\nexample.com";
            var result = _facade.ValidateRegistration(ValidationField.Email, email);

            Assert.False(result.IsValid);
        }
        [Fact]

        public void ValidateField_WithWhiteSpaceValue_ShouldReturnFailure()
        {
            string email = "  ";
            var result = _facade.ValidateRegistration(ValidationField.Email, email);

            Assert.False(result.IsValid);
        }
        [Fact]

        public void ValidateField_WithInvalidEmail_ShouldReturnFailure() 
        {
            string email = "invalid13";

            var result = _facade.ValidateRegistration(ValidationField.Email, email);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Email") || e.Contains("format"));
        }
        [Fact]
        public async Task ValidateField_WithValidPassword_ShouldReturnSuccess() 
        {
            string password = "PasswordStrongEnough123!";
            var result = await _facade.ValidateRegistrationAsync(ValidationField.Password, password);

            Assert.True(result.IsValid);
        }
        [Fact]
        public async Task ValidateField_WithWeakPassword_ShouldReturnFailure() 
        {
            string password = "weak";

            var result =await _facade.ValidateRegistrationAsync(ValidationField.Password, password);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Password"));
            Assert.True(result.Errors.Count > 1);
        }
        [Theory]
        [InlineData("José")]              // Spanish accent
        [InlineData("François")]          // French accent
        [InlineData("Müller")]            // German umlaut
        [InlineData("O'Connor")]          // Irish apostrophe
        [InlineData("Mary-Anne")]         // Hyphen
        [InlineData("Jean-Luc")]          // Hyphen
        [InlineData("Van Der Berg")]
        public void ValidateField_WithInternationalNames_ShouldReturnSuccess(string name) 
        {

            var result = _facade.ValidateRegistration(ValidationField.FirstName, name);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData ("John123")]
        [InlineData("!!!")]
        [InlineData("John!")]// for sure this
        [InlineData("J")]
       public void ValidateField_WithInvalidNames_ShouldReturnFailure(string name) 
        {
            var result = _facade.ValidateRegistration(ValidationField.FirstName, name);

            Assert.False(result.IsValid);
        }
        [Fact]
        public void ValidateField_WithValidFirstName_ShouldReturnTrue() 
        {
            string name = "John";
            var result = _facade.ValidateRegistration(ValidationField.FirstName, name);
            Assert.True(result.IsValid);
        }
        [Fact]
        public void ValidateField_WithValidLastName_ShouldReturnTrue()
        {
            string name = "Doe";
            var result = _facade.ValidateRegistration(ValidationField.LastName, name);
            Assert.True(result.IsValid);
        }
        [Fact]
        public void ValidateField_WithTooShortFName_ShouldReturnFailure() 
        {
            string name = "j";

            var result = _facade.ValidateRegistration(ValidationField.FirstName, name);

            Assert.False(result.IsValid);
          
        }
        [Fact]
        public void ValidateField_WithTooShortLName_ShouldReturnFailure()
        {
            string name = "j";

            var result = _facade.ValidateRegistration(ValidationField.LastName, name);

            Assert.False(result.IsValid);
          
        }

        [Fact]
        public void ValidateField_WithValidPhoneNumber_ShouldReturnTrue() 
        {
            var phoneNumber = "1234567890";
            var result = _facade.ValidateRegistration(ValidationField.PhoneNumber, phoneNumber);

            Assert.True(result.IsValid);
        }
        [Fact]
        public void ValidateField_WithPhoneNumber_ShouldReturnFailure() 
        {
            var phoneNumber = "123";
            var result = _facade.ValidateRegistration(ValidationField.PhoneNumber, phoneNumber);

            Assert.False(result.IsValid);
            
        }
        [Fact]
        public async Task ValidateRegistration_WithAllValidFields_ShouldReturnTrue() 
        {
            var dto = new RegisterDto
            {
                EmailAddress ="test@example.com",
                Password = "PasswordStrongEnough123!",
                FirstName="John",
                LastName = "Doe",
                PhoneNumber ="1234567890"
            };

            var result = await _facade.ValidateRegistration(dto);
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
        [Fact]
        public async Task ValidateRegistration_WithAllInvalidFields_ShouldReturnFailure() 
        {
            var dto = new RegisterDto
            {
                EmailAddress = "test",
                Password = "Pa",
                FirstName = "J",
                LastName = "D",
                PhoneNumber = "1234"
            };

            var result =  await _facade.ValidateRegistration(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Email"));
            Assert.Contains(result.Errors, e => e.Contains("PhoneNumber"));
            Assert.Contains(result.Errors, e => e.Contains("Password"));
            Assert.Contains(result.Errors, e => e.Contains("FirstName"));
            Assert.Contains(result.Errors, e => e.Contains("LastName"));

        }
        [Fact]
        public async Task ValidateRegistration_WithMixedInvalidFields_ShouldReturnFailureAndSpecificErrors()
        {
            var dto = new RegisterDto
            {
                EmailAddress = "test@example.com",
                Password = "weak",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "123"
            };

            var result = await _facade.ValidateRegistration(dto);
            Assert.False(result.IsValid);

            Assert.Contains(result.Errors, e => e.Contains("PhoneNumber"));
            Assert.Contains(result.Errors, e => e.Contains("Password"));



        }
        [Fact]
        public async Task ValidateRegistration_WithOneInvalidFileds_ShouldReturnFailureAndSpecificErrors() 
        {
            var dto = new RegisterDto
            {
                EmailAddress = "test@example.com",
                Password = "weak",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "1234567890"
            };

            var result = await _facade.ValidateRegistration(dto);
            Assert.False(result.IsValid);

       
            Assert.Contains(result.Errors, e => e.Contains("Password"));
        }


    }
}
