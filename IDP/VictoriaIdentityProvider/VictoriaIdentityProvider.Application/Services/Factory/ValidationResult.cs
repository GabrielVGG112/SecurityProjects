namespace VictoriaIdentityProvider.Application.Services.Factory
{
    public class ValidationResult
    {
        public bool IsValid { get; init; }
        public List<string> Errors { get; init; } =[ ];

        public static ValidationResult Success() => new() { IsValid = true };
        public static ValidationResult Failure(string error) => new() { IsValid = false, Errors = [error] };
        public static ValidationResult Failure(List<string> errors) => new() { IsValid = false, Errors = errors };
    }
}