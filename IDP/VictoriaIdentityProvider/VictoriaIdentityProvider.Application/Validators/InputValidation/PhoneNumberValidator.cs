using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using VictoriaIdentityProvider.Application.Interfaces.ValidatorInterfaces;
using ValidationResult = VictoriaIdentityProvider.Application.Services.Factory.ValidationResult;
namespace VictoriaIdentityProvider.Application.Validators.InputValidation
{
    public class PhoneNumberValidator : IValidator<string>
    {
        private readonly ILogger<PhoneNumberValidator>? _logger;
        public PhoneNumberValidator() :this(null) { }

        public PhoneNumberValidator(ILogger<PhoneNumberValidator>? logger)
        {
            _logger = logger;
        }
        public ValidationResult Validate(string phoneNumber) 
        {
         
            var phoneWithoutSpace = phoneNumber?.Trim() ?? string.Empty;
            var phone = new PhoneAttribute();
            var errors = new List<string>();

         
            if (string.IsNullOrWhiteSpace(phoneWithoutSpace))
                return ValidationResult.Failure("Phone number is required");

          
            if (phoneWithoutSpace.Length > 20)
                errors.Add("Phone number is too long");


            if (phoneWithoutSpace.Any(c => char.IsControl(c)))
            {
                _logger?.LogCritical($"Attemt to use Control Characters {nameof(phoneNumber)}");
                errors.Add("Phone number contains invalid control characters");
            }
         
            if (phoneWithoutSpace.Any(c => char.IsLetter(c)))
                errors.Add("Phone number can't contain letters");

          
            if (phoneWithoutSpace.Length < 10)
                errors.Add("Phone number is too short");

       
            if (!phone.IsValid(phoneWithoutSpace))
                errors.Add("Invalid phone number format");

            return errors.Any() 
                ? ValidationResult.Failure(errors) 
                : ValidationResult.Success();
        }
    }
}
