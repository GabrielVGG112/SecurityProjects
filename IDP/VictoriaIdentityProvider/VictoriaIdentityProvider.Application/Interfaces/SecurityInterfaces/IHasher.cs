namespace VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;

/// <summary>
/// Defines the contract for password hashing operations.
/// </summary>
public interface IHasher
{
    /// <summary>
    /// Hashes a plain text password using a secure hashing algorithm.
    /// </summary>
    /// <param name="password">The plain text password to hash.</param>
    /// <returns>The hashed password.</returns>
    string HashData(string data);

    /// <summary>
    /// Verifies if a plain text password matches a hashed password.
    /// </summary>
    /// <param name="password">The plain text password to verify.</param>
    /// <param name="hashedPassword">The hashed password to compare against.</param>
    /// <returns>True if the password matches; otherwise, false.</returns>
    bool VerifyData(string userData, string dbHashedData);
}
