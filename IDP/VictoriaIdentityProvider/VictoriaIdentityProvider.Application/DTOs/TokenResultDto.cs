using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Application.DTOs
{
    public record TokenResultDto
    {
    

        public string JwtToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } =string.Empty;
        public DateTime ValidUntil { get; set; }
     
    }
}
