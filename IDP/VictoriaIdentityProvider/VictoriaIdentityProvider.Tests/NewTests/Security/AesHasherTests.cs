using Microsoft.Extensions.Configuration;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;
using VictoriaIdentityProvider.Infrastructure.Security;

namespace VictoriaIdentityProvider.Tests.NewTests.Security;

public class AesHasherTests
{
    private readonly ICipher _aesHasher;

    public AesHasherTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Security:Aes:Key", Convert.ToBase64String(new byte[32]) }
            })
            .Build();

        _aesHasher = new AesHasher(configuration);
    }

    [Fact]
    public void Encrypt_WithValidPlainText_ShouldReturnEncryptedString()
    {
        var plainText = "test@email.com";

        var encrypted = _aesHasher.Encrypt(plainText);

        Assert.NotNull(encrypted);
        Assert.NotEqual(plainText, encrypted);
        Assert.Contains(":", encrypted);
    }

    [Fact]
    public void Decrypt_WithValidEncryptedText_ShouldReturnOriginalPlainText()
    {
        var plainText = "test@email.com";
        var encrypted = _aesHasher.Encrypt(plainText);

        var decrypted = _aesHasher.Decrypt(encrypted);

        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_WithEmptyString_ShouldReturnEmptyString()
    {
        var plainText = "";

        var encrypted = _aesHasher.Encrypt(plainText);
        var decrypted = _aesHasher.Decrypt(encrypted);

        Assert.Equal(plainText, encrypted);
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_WithSpecialCharacters_ShouldWorkCorrectly()
    {
        var plainText = "Test@123!#$%^&*()_+{}|:\"<>?";

        var encrypted = _aesHasher.Encrypt(plainText);
        var decrypted = _aesHasher.Decrypt(encrypted);

        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Decrypt_WithInvalidFormat_ShouldThrowFormatException()
    {
        var invalidEncrypted = "InvalidFormat";

        Assert.Throws<FormatException>(() => _aesHasher.Decrypt(invalidEncrypted));
    }

    [Fact]
    public void Encrypt_SameTextTwice_ShouldProduceDifferentResults()
    {
        var plainText = "test@email.com";

        var encrypted1 = _aesHasher.Encrypt(plainText);
        var encrypted2 = _aesHasher.Encrypt(plainText);

        Assert.NotEqual(encrypted1, encrypted2);
    }
}
