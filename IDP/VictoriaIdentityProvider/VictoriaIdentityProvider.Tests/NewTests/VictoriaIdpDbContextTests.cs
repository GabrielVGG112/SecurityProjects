using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VictoriaIdentityProvider.Domain.Models;
using VictoriaIdentityProvider.Infrastructure.Config.Interceptors;
using VictoriaIdentityProvider.Infrastructure.DbConnection;
using VictoriaIdentityProvider.Infrastructure.Security;

namespace VictoriaIdentityProvider.Tests.NewTests;

public class VictoriaIdpDbContextTests : IDisposable
{
    private readonly VictoriaIdpDbContext _context;
    private readonly AesHasher _aesHasher;

    public VictoriaIdpDbContextTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Security:Aes:Key", Convert.ToBase64String(new byte[32]) }
            })
            .Build();

        _aesHasher = new AesHasher(configuration);

        var options = new DbContextOptionsBuilder<VictoriaIdpDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestContextDb_{Guid.NewGuid()}")
            .Options;
        var interceptor = new AuditInterceptor();
        _context = new VictoriaIdpDbContext(options, _aesHasher, interceptor);
    }

    [Fact]
    public void DbContext_ShouldHaveUsersDbSet()
    {
        Assert.NotNull(_context.Users);
    }

    [Fact]
    public async Task DbContext_CanAddAndSaveUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "dbcontext@example.com",
            FirstName = "DbContext",
            LastName = "Test",
            IsEmailConfirmed = false,
            FailedLoginAttempts = 0
        };

        _context.Users.Add(user);
        var result = await _context.SaveChangesAsync();

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task DbContext_CanQueryUsers()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "query@example.com",
            FirstName = "Query",
            LastName = "Test",
            IsEmailConfirmed = false,
            FailedLoginAttempts = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var users = await _context.Users.ToListAsync();

        Assert.NotEmpty(users);
        Assert.Contains(users, u => u.Email == "query@example.com");
    }

    [Fact]
    public async Task DbContext_EmailEncryption_WorksCorrectly()
    {
        var plainEmail = "encryption@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = plainEmail,
            FirstName = "Encryption",
            LastName = "Test",
            IsEmailConfirmed = false,
            FailedLoginAttempts = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == plainEmail);

        Assert.NotNull(savedUser);
        Assert.Equal(plainEmail, savedUser.Email);
    }

    [Fact]
    public async Task DbContext_CanUpdateUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "update@example.com",
            FirstName = "Original",
            LastName = "Name",
            IsEmailConfirmed = false,
            FailedLoginAttempts = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        user.FirstName = "Updated";
        await _context.SaveChangesAsync();

        var updatedUser = await _context.Users.FindAsync(user.Id);

        Assert.NotNull(updatedUser);
        Assert.Equal("Updated", updatedUser.FirstName);
    }

    [Fact]
    public async Task DbContext_CanDeleteUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "delete@example.com",
            FirstName = "Delete",
            LastName = "Test",
            IsEmailConfirmed = false,
            FailedLoginAttempts = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        var deletedUser = await _context.Users.FindAsync(user.Id);

        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task DbContext_MultipleUsers_WithDifferentEmails_ShouldSaveSuccessfully()
    {
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Email = "user1@example.com",
                FirstName = "User",
                LastName = "One",
                IsEmailConfirmed = false,
                FailedLoginAttempts = 0
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "user2@example.com",
                FirstName = "User",
                LastName = "Two",
                IsEmailConfirmed = false,
                FailedLoginAttempts = 0
            }
        };

        _context.Users.AddRange(users);
        var result = await _context.SaveChangesAsync();

        Assert.Equal(2, result);
    }

    [Fact]
    public async Task DbContext_TrackingBehavior_AsNoTracking_ShouldNotTrackEntities()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "tracking@example.com",
            FirstName = "Tracking",
            LastName = "Test",
            IsEmailConfirmed = false,
            FailedLoginAttempts = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var trackedUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "tracking@example.com");

        Assert.NotNull(trackedUser);
        Assert.Equal(EntityState.Detached, _context.Entry(trackedUser).State);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

