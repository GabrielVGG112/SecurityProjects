using Microsoft.Extensions.Options;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;
using VictoriaIdentityProvider.Infrastructure.Config.OptionsModels;
using VictoriaIdentityProvider.Infrastructure.Security;

namespace VictoriaIdentityProvider.Tests.NewTests.Security;

public class Argon2HasherTests
{
    private readonly IHasher _argon2Hasher;

    public Argon2HasherTests()
    {
        var options = Options.Create(new Argon2Options
        {
            SaltSize = 16,
            HashSize = 32,
            Iterations = 3,
            MemorySize = 65536,
            Paralelism = 1,
            Pepper = "TestPepper123",
        });

        _argon2Hasher = new Argon2Hasher(options);
    }

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHashedPassword()
    {
        var password = "TestPassword123!";

        var hashedPassword = _argon2Hasher.HashData(password);

        Assert.NotNull(hashedPassword);
        Assert.NotEqual(password, hashedPassword);
        Assert.StartsWith("$argon2id$v=19$", hashedPassword);
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ShouldProduceDifferentHashes()
    {
        var password = "TestPassword123!";

        var hash1 = _argon2Hasher.HashData(password);
        var hash2 = _argon2Hasher.HashData(password);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var password = "TestPassword123!";
        var hashedPassword = _argon2Hasher.HashData(password);

        var result = _argon2Hasher.VerifyData(password, hashedPassword);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hashedPassword = _argon2Hasher.HashData(password);

        var result = _argon2Hasher.VerifyData(wrongPassword, hashedPassword);

        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldReturnFalse()
    {
        var password = "TestPassword123!";
        var hashedPassword = _argon2Hasher.HashData(password);

        var result = _argon2Hasher.VerifyData("", hashedPassword);

        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithInvalidHashFormat_ShouldReturnFalse()
    {
        var password = "TestPassword123!";
        var invalidHash = "InvalidHashFormat";

        var result = _argon2Hasher.VerifyData(password, invalidHash);

        Assert.False(result);
    }

    [Fact]
    public void HashPassword_WithSpecialCharacters_ShouldWorkCorrectly()
    {
        var password = "P@ssw0rd!#$%^&*()_+{}|:\"<>?";

        var hashedPassword = _argon2Hasher.HashData(password);
        var result = _argon2Hasher.VerifyData(password, hashedPassword);

        Assert.True(result);
    }
}
