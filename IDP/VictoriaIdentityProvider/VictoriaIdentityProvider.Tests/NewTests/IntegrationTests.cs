using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using VictoriaIdentityProvider.Domain.Models;
using VictoriaIdentityProvider.Infrastructure.Config.Interceptors;
using VictoriaIdentityProvider.Infrastructure.Config.OptionsModels;
using VictoriaIdentityProvider.Infrastructure.DbConnection;
using VictoriaIdentityProvider.Infrastructure.Repositories;
using VictoriaIdentityProvider.Infrastructure.Security;

namespace VictoriaIdentityProvider.Tests.NewTests;

/// <summary>
/// Integration tests that test the complete flow of user registration with encryption and hashing
/// </summary>
public class IntegrationTests : IDisposable
{
    private readonly VictoriaIdpDbContext _context;
    private readonly AesHasher _aesHasher;
    private readonly Argon2Hasher _argon2Hasher;
    private readonly UserRepository _userRepository;

    public IntegrationTests()
    {
    
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Security:Aes:Key", Convert.ToBase64String(new byte[32]) }
            })
            .Build();

        _aesHasher = new AesHasher(configuration);

       
        var argon2Options = Options.Create(new Argon2Options
        {
            SaltSize = 16,
            HashSize = 32,
            Iterations = 3,
            MemorySize = 65536,
            Paralelism = 1,
            Pepper = "IntegrationTestPepper"
        });

        _argon2Hasher = new Argon2Hasher(argon2Options);

     
        var dbOptions = new DbContextOptionsBuilder<VictoriaIdpDbContext>()
            .UseInMemoryDatabase(databaseName: $"IntegrationTestDb_{Guid.NewGuid()}")
            .Options;
        var interceptor = new AuditInterceptor();
        _context = new VictoriaIdpDbContext(dbOptions, _aesHasher, interceptor);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task CompleteUserRegistration_WithEncryptedEmail_ShouldWorkCorrectly()
    {
       
        var plainEmail = "integration@example.com";
        var plainPassword = "SecurePassword123!";

     
        var hashedPassword = _argon2Hasher.HashData(plainPassword);

        var user = new User
        {
            Email = plainEmail,
            FirstName = "Integration",
            LastName = "Test",
            IsEmailConfirmed = false,
            FailedLoginAttempts = 0
        };

       
        await _userRepository.AddUserAsync(user);
        var retrievedUser = await _userRepository.GetUserByEmailAsync(plainEmail);

       
        Assert.NotNull(retrievedUser);
        Assert.Equal(plainEmail, retrievedUser.Email);
        Assert.Equal("Integration", retrievedUser.FirstName);
        Assert.Equal("Test", retrievedUser.LastName);
        Assert.NotNull(hashedPassword);
        Assert.StartsWith("$argon2id$v=19$", hashedPassword);
    }

    [Fact]
    public async Task AesEncryption_OnEmailField_ShouldWorkTransparently()
    {
        
        var plainEmail = "encrypted@example.com";
        var user = new User
        {
            Email = plainEmail,
            FirstName = "Encrypted",
            LastName = "User"
        };

     
        await _userRepository.AddUserAsync(user);
        
      
        var retrievedUser = await _userRepository.GetUserByEmailAsync(plainEmail);

        // Assert
        Assert.Equal(plainEmail, retrievedUser.Email);
        Assert.Equal("Encrypted", retrievedUser.FirstName);
    }

    [Fact]
    public async Task MultipleUsers_WithDifferentEncryptedEmails_ShouldAllBeRetrievable()
    {
       
        var users = new List<User>
        {
            new User { Email = "user1@integration.com", FirstName = "User", LastName = "One" },
            new User { Email = "user2@integration.com", FirstName = "User", LastName = "Two" },
            new User { Email = "user3@integration.com", FirstName = "User", LastName = "Three" }
        };

       
        foreach (var user in users)
        {
            await _userRepository.AddUserAsync(user);
        }

       
        var user1 = await _userRepository.GetUserByEmailAsync("user1@integration.com");
        var user2 = await _userRepository.GetUserByEmailAsync("user2@integration.com");
        var user3 = await _userRepository.GetUserByEmailAsync("user3@integration.com");

        Assert.Equal("User", user1.FirstName);
        Assert.Equal("One", user1.LastName);
        Assert.Equal("User", user2.FirstName);
        Assert.Equal("Two", user2.LastName);
        Assert.Equal("User", user3.FirstName);
        Assert.Equal("Three", user3.LastName);
    }

    [Fact]
    public void AesHasher_EncryptionDecryption_RoundTrip_ShouldPreserveData()
    {
       
        var sensitiveData = "sensitive@email.com";

        var encrypted = _aesHasher.Encrypt(sensitiveData);
        var decrypted = _aesHasher.Decrypt(encrypted);

        
        Assert.NotEqual(sensitiveData, encrypted);
        Assert.Equal(sensitiveData, decrypted);
    }

    [Fact]
    public async Task DatabaseContext_WithAesHasher_ShouldInitializeCorrectly()
    {
        
        Assert.NotNull(_context);
        Assert.NotNull(_context.Users);
        Assert.True(await _context.Database.CanConnectAsync());
    }

    [Fact]
    public async Task UserRepository_SaveAndRetrieve_WithAllUserProperties_ShouldWorkCorrectly()
    {
     
        var now = DateTime.UtcNow;
        var user = new User
        {
            Email = "complete@integration.com",
            FirstName = "Complete",
            LastName = "User",
            IsEmailConfirmed = true,
            
            
            FailedLoginAttempts = 0,
            LockoutEnd = null,
            LastLogInAt = now
        };

     
        await _userRepository.AddUserAsync(user);
        var retrieved = await _userRepository.GetUserByEmailAsync("complete@integration.com");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(user.Email, retrieved.Email);
        Assert.Equal(user.FirstName, retrieved.FirstName);
        Assert.Equal(user.LastName, retrieved.LastName);
        Assert.Equal(user.IsEmailConfirmed, retrieved.IsEmailConfirmed);
        Assert.Equal(user.FailedLoginAttempts, retrieved.FailedLoginAttempts);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}




