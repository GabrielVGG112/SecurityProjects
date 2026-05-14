namespace VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;

/// <summary>
/// Defines the contract for encryption and decryption operations.
/// </summary>
public interface ICipher
{
    /// <summary>
    /// Encrypts plain text using a symmetric encryption algorithm.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted text.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts cipher text using a symmetric encryption algorithm.
    /// </summary>
    /// <param name="cipherText">The encrypted text to decrypt.</param>
    /// <returns>The decrypted plain text.</returns>
    string Decrypt(string cipherText);
}
