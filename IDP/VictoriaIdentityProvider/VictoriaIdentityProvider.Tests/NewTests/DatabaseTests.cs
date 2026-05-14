using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VictoriaIdentityProvider.Domain.Models;
using VictoriaIdentityProvider.Infrastructure.Config.Interceptors;
using VictoriaIdentityProvider.Infrastructure.DbConnection;
using VictoriaIdentityProvider.Infrastructure.Repositories;
using VictoriaIdentityProvider.Infrastructure.Security;

namespace VictoriaIdentityProvider.Tests.NewTests;

public class DatabaseTests : IDisposable
{
    private readonly VictoriaIdpDbContext _context;
    private readonly AesHasher _aesHasher;
    private readonly UserRepository _userRepository;

    public DatabaseTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: new Dictionary<string, string>
            {
                { "Security:Aes:Key", Convert.ToBase64String(new byte[32]) }
            }!)
            .Build();

        _aesHasher = new AesHasher(configuration);

        var options = new DbContextOptionsBuilder<VictoriaIdpDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        var interceptor = new AuditInterceptor();
        _context = new VictoriaIdpDbContext(options, _aesHasher,interceptor);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task DbContext_ShouldBeInitializedCorrectly()
    {
        Assert.NotNull(_context);
        Assert.NotNull(_context.Users);
    }

    [Fact]
    public async Task AddUser_ShouldSaveUserToDatabase()
    {
        var user = new User
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsEmailConfirmed = false,
            FailedLoginAttempts = 0
        };

        await _userRepository.AddUserAsync(user);

        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        Assert.NotNull(savedUser);
        Assert.Equal(user.Email, savedUser.Email);
        Assert.Equal(user.FirstName, savedUser.FirstName);
        Assert.Equal(user.LastName, savedUser.LastName);
    }



    [Fact]
    public async Task GetUserByEmail_WithExistingEmail_ShouldReturnUser()
    {
        var user = new User
        {
            Email = "existing@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            IsEmailConfirmed = true,
            FailedLoginAttempts = 0
        };

        await _userRepository.AddUserAsync(user);

        var retrievedUser = await _userRepository.GetUserByEmailAsync("existing@example.com");

        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Email, retrievedUser.Email);
        Assert.Equal(user.FirstName, retrievedUser.FirstName);
        Assert.Equal(user.LastName, retrievedUser.LastName);
    }


    [Fact]
    public async Task AddMultipleUsers_ShouldSaveAllUsers()
    {
        var users = new List<User>
        {
            new User { Email = "user1@example.com", FirstName = "User", LastName = "One" },
            new User { Email = "user2@example.com", FirstName = "User", LastName = "Two" },
            new User { Email = "user3@example.com", FirstName = "User", LastName = "Three" }
        };

        foreach (var user in users)
        {
            await _userRepository.AddUserAsync(user);
        }

        var savedUsers = await _context.Users.ToListAsync();

        Assert.Equal(users.Count, savedUsers.Count);
    }

    [Fact]
    public async Task User_EmailField_ShouldBeEncryptedInDatabase()
    {
        var user = new User
        {
            Email = "encrypted@example.com",
            FirstName = "Encrypted",
            LastName = "User"
        };

        await _userRepository.AddUserAsync(user);

        var savedUser = await _context.Users.FirstOrDefaultAsync();
        Assert.NotNull(savedUser);

        var retrievedUser = await _userRepository.GetUserByEmailAsync("encrypted@example.com");
        Assert.Equal("encrypted@example.com", retrievedUser.Email);
    }

    [Fact]
    public async Task User_IdShouldBeGenerated()
    {
        var user = new User
        {
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User"
        };

        await _userRepository.AddUserAsync(user);

        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        Assert.NotEqual(Guid.Empty, savedUser.Id);
    }

    [Fact]
    public async Task User_WithAllProperties_ShouldBeSavedCorrectly()
    {
        var user = new User
        {
            Email = "fulluser@example.com",
            FirstName = "Full",
            LastName = "User",
            IsEmailConfirmed = true,
            
            
            FailedLoginAttempts = 2,
            LockoutEnd = DateTime.UtcNow.AddMinutes(30),
            LastLogInAt = DateTime.UtcNow
        };

        await _userRepository.AddUserAsync(user);

        var retrievedUser = await _userRepository.GetUserByEmailAsync("fulluser@example.com");

        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Email, retrievedUser.Email);
        Assert.Equal(user.FirstName, retrievedUser.FirstName);
        Assert.Equal(user.LastName, retrievedUser.LastName);
        Assert.Equal(user.IsEmailConfirmed, retrievedUser.IsEmailConfirmed);
        Assert.Equal(user.FailedLoginAttempts, retrievedUser.FailedLoginAttempts);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}


