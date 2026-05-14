using System.ComponentModel.DataAnnotations;

namespace VictoriaIdentityProvider.Application.DTOs
{
    public record ResetPasswordDto
    {
        [Required]
        public string NewPassword { get; set; } = string.Empty;
        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
