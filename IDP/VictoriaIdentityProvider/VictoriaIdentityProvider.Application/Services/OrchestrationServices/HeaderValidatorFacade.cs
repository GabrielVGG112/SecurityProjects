using System.Text;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Enums;
using VictoriaIdentityProvider.Application.Services.Factory;
using VictoriaIdentityProvider.Application.Validators;
using VictoriaIdentityProvider.Application.Validators.HeaderValidation;

namespace VictoriaIdentityProvider.Application.Services.OrchestrationServices
{
    public class HeaderValidatorFacade
    {
        private readonly Dictionary<ValidationField, Func<string, Task<ValidationResult>>> _validators;
       protected HeaderValidatorFacade() { _validators = []; }
       public HeaderValidatorFacade (
           RefreshTokenValidator refreshTokenValidator,
           JwtTokenValidator jwtValidator,
           SessionValidator sessionValidator
           )
        {
            _validators = [];
            _validators[ValidationField.RefreshToken] = refreshTokenValidator.Validate;
            _validators[ValidationField.JwtToken] = jwtValidator.Validate;
            _validators[ValidationField.Session] = sessionValidator.Validate;
        }

        public virtual async Task<ValidationResult> ValidateRegistration(ValidationField field, string value)
        {
            var errors = new List<string>();
            await AddErrors(field, value, errors);

            if (errors.Count != 0)
            {
                return ValidationResult.Failure(errors);
            }
            else
            {
                return ValidationResult.Success();
            }
        }
     
        public virtual async Task<ValidationResult> ValidateRegistration(TokenResultDto dto) 
        {
            var errors = new List<string>();
            await AddErrors(ValidationField.JwtToken, dto.JwtToken, errors);
            await AddErrors(ValidationField.RefreshToken, dto.RefreshToken, errors);
            
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
        private async Task AddErrors(ValidationField field, string value, List<string> allErrors)
        {
            if (!_validators.TryGetValue(field, out var validator))
                throw new InvalidOperationException(
            $"No validator registered for field: {field}. Check DI registration.");
            var result = await  validator(value);

            var fieldName = field.ToString();
            allErrors.AddRange(result.Errors.Select(e => $"{fieldName} : {e}"));
        }
    }
}
