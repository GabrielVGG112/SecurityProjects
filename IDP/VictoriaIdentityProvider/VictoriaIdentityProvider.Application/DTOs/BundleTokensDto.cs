using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Application.DTOs
{
    public record BundleTokensDto(
        string JwtToken,
        string RefreshToken,
        string RefreshTokenHash,
        int expirationDays);
  
}
