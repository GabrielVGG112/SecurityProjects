using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VictoriaIdentityProvider.Application.DTOs
{
    public record RegisterDto
    {
        [Required]

        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;    
        [Required]
        public string Password { get; set; } = string.Empty;

        [Compare(nameof(Password) ,ErrorMessage ="Password doesn't match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
        [Phone]
        public string PhoneNumber { get; set; }=string.Empty;
    }
}
