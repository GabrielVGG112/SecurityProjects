using System.Text;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Enums;
using VictoriaIdentityProvider.Application.Services.Factory;
using VictoriaIdentityProvider.Application.Validators.InputValidation;

namespace VictoriaIdentityProvider.Application.Services.OrchestrationServices
{
    public class FieldValidatorFacade 
    {
        private readonly Dictionary<ValidationField, Func<string, ValidationResult>> _validators;
        private readonly Dictionary<ValidationField, Func<string, Task<ValidationResult>>> _asyncValidators;
        public FieldValidatorFacade(
        EmailValidator emailValidator,
        PasswordValidator passwordValidator,
        NameValidator nameValidator,
        PhoneNumberValidator phoneValidator
       
            )
        {
            _asyncValidators = [];
            _validators = [];
            _validators[ValidationField.Email] = emailValidator.Validate;
            _asyncValidators[ValidationField.Password] = passwordValidator.Validate;
            _validators[ValidationField.FirstName] = nameValidator.Validate;
            _validators[ValidationField.LastName] = nameValidator.Validate;
            _validators[ValidationField.PhoneNumber] = phoneValidator.Validate;

        }

        public ValidationResult ValidateRegistration(ValidationField field , string value) 
        {
            var errors = new List<string>();
            AddErrors(field, value, errors);

            if (errors.Count != 0)
            {
                return ValidationResult.Failure(errors);
            }
            else
            {
                return ValidationResult.Success();
            }
        }
        public async Task<ValidationResult> ValidateRegistrationAsync(ValidationField field ,string value) 
        {
            var errors = new List<string>();
            await AddErrorsAsync(field, value, errors);

            if (errors.Count != 0)
            {
                return ValidationResult.Failure(errors);
            }
            else
            {
                return ValidationResult.Success();
            }
        }
        public async Task<ValidationResult> ValidateRegistration(RegisterDto dto)
        {
            var errors = new List<string>();
            AddErrors(ValidationField.Email, dto.EmailAddress, errors);
            AddErrors(ValidationField.FirstName, dto.FirstName, errors);
            AddErrors(ValidationField.LastName, dto.LastName, errors);
            AddErrors(ValidationField.PhoneNumber, dto.PhoneNumber, errors);

            await AddErrorsAsync(ValidationField.Password, dto.Password, errors);

            if (errors.Any())
            {
                var fieldNames = new Dictionary<string, int>();
                StringBuilder builder = new StringBuilder();

                fieldNames = errors
                    .GroupBy(e => e.Split(':', 2)[0].Trim())
                    .ToDictionary(g => g.Key, g => g.Count());

                foreach (var (key, value) in fieldNames)
                {
                    builder.AppendLine(string.Format("{0} : {1}", key, value));
                }


                return ValidationResult.Failure(errors);
            }
            else
            {
                return ValidationResult.Success();
            }
        }
        private void AddErrors(ValidationField field, string value, List<string> allErrors)
        {
            if (!_validators.TryGetValue(field, out var validator))
                throw new InvalidOperationException(
            $"No validator registered for field: {field}. Check DI registration.");
            var result = validator(value);

            var fieldName = field.ToString();
            allErrors.AddRange(result.Errors.Select(e => $"{fieldName} : {e}"));
        }
        private async Task AddErrorsAsync(ValidationField field, string value, List<string> allErrors)
        {
            if (!_asyncValidators.TryGetValue(field, out var validator))
                throw new InvalidOperationException(
            $"No validator registered for field: {field}. Check DI registration.");
            var result = await validator(value);

            var fieldName = field.ToString();
            allErrors.AddRange(result.Errors.Select(e => $"{fieldName} : {e}"));
        }
    }
}


