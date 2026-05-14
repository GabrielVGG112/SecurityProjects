using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces
{
    /// <summary>
    /// Defines methods for generating and validating web tokens used for user authentication and authorization.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for creating secure tokens that can be used
    /// to identify users and for validating tokens to ensure their authenticity and integrity. The specific token
    /// format and validation logic may vary depending on the implementation.</remarks>
    public interface IJwtInternalToken
    {
        /// <summary>
        /// Validates a JSON Web Token (JWT) and returns the associated claims principal.
        /// </summary>
        /// <remarks>The method verifies the token's signature and expiration. The caller should ensure
        /// the token is obtained from a trusted source. If validation fails, the returned ClaimsPrincipal may not be
        /// authenticated.</remarks>
        /// <param name="token">The JWT to validate. Cannot be null or empty.</param>
        /// <returns>A ClaimsPrincipal representing the authenticated user and associated claims if the token is valid;
        /// otherwise, an unauthenticated principal or null, depending on implementation.</returns>
        Guid? ValidateJwtInternalToken(string token, string expectedPurpose);

        /// <summary>
        /// Generates a JSON Web Token (JWT) using the specified claims and a symmetric signing key.
        /// </summary>
        /// <remarks>The caller is responsible for ensuring that the claims provided are appropriate for
        /// the intended audience and use case. The method does not validate the claims' content or structure.</remarks>
        /// <param name="claims">The collection of claims to include in the generated JWT. Cannot be null or empty.</param>
        /// <returns>A string containing the generated JWT. The token is signed using a symmetric key and encodes the provided
        /// claims.</returns>
      
        string GenerateJwtInternalToken(string purpose, Guid userId, int hours);
    }
}
