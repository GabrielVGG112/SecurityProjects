using Microsoft.Extensions.Logging;
using VictoriaIdentityProvider.Application.Interfaces.ValidatorInterfaces;
using VictoriaIdentityProvider.Application.Services.Factory;

namespace VictoriaIdentityProvider.Application.Validators.InputValidation
{
    public class NameValidator : IValidator<string>
    {

        public ValidationResult Validate(string name) 
        {
            string nameWithoutSpace = name?.Trim() ?? string.Empty;
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(nameWithoutSpace))
                return ValidationResult.Failure("Name is required");

            if (nameWithoutSpace.Length > 100)
                return ValidationResult.Failure("Name must not exceed 100 characters");

            if (nameWithoutSpace.Length < 2) 
                return ValidationResult.Failure("Name must be at least 2 characters");

            if (nameWithoutSpace.Any(c => char.IsControl(c)))
                errors.Add("Invalid name format");

            if (nameWithoutSpace.Any(c => char.IsDigit(c)))
            {
                
                errors.Add("Name can't contain digits");
            }
      
            if (!nameWithoutSpace.Any(c => char.IsLetter(c)))
                errors.Add("Name must contain at least one letter");

     
            if (nameWithoutSpace.Any(c => !char.IsLetter(c) && c != ' ' && c != '-' && c != '\''))
                errors.Add("Name can only contain letters, spaces, hyphens, and apostrophes");

            return errors.Any() ? 
                ValidationResult.Failure(errors) :
                ValidationResult.Success();
        }
    }
}
