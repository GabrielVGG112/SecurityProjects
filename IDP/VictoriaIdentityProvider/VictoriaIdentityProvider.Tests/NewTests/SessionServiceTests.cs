using Moq;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Enums;
using VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;
using VictoriaIdentityProvider.Application.Services.ClientServices;
using VictoriaIdentityProvider.Application.Services.Factory;
using VictoriaIdentityProvider.Application.Services.OrchestrationServices;
using VictoriaIdentityProvider.Domain.CustomErrors;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Tests.NewTests;

public class SessionServiceTests
{
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IJwtAccessTokenService> _mockJwtService;
    private readonly Mock<ILoginService> _mockLoginService;
    private readonly Mock<ModelFactoryFacade> _mockModelFactory;
    private readonly Mock<HeaderValidatorFacade> _mockValidator;

    private readonly SessionService _sessionService;

    private readonly User _user;
    private readonly UserSession _session;
    private readonly RefreshToken _refreshToken;

    public SessionServiceTests()
    {
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockJwtService = new Mock<IJwtAccessTokenService>();
        _mockLoginService = new Mock<ILoginService>();
        _mockModelFactory = new Mock<ModelFactoryFacade>();
        _mockValidator = new Mock<HeaderValidatorFacade>();

        _user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsEmailConfirmed = true,
            UserStatus = UserStatusEnum.Active
        };

        _session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            User = _user,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            SessionToken = "hashed-session-token"
        };

        _refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            User = _user,
            TokenHash = "hashed-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _sessionService = new SessionService(
            _mockRepositoryFactory.Object,
            _mockJwtService.Object,
            _mockLoginService.Object,
            _mockModelFactory.Object,
            _mockValidator.Object
        );
    }

    [Fact]
    public async Task GenerateNewSessionAsync_WithValidLogin_ShouldReturnTokenResult()
    {
        var dto = new LoginDto { Email = "test@example.com", Password = "SecurePass123!" };
        var rawToken = "raw-refresh-token";

        _mockLoginService.Setup(s => s.LoginUserAsync(dto)).ReturnsAsync(_user);
        _mockValidator.Setup(v => v.ValidateRegistration(ValidationField.Session, _user.Id.ToString()))
            .ReturnsAsync(ValidationResult.Success());
        _mockRepositoryFactory.Setup(r => r.GetMultipleDependenciesAsync<UserSession>(
            InstanceNamesEnum.UserSession, _user.Id))
            .ReturnsAsync(Enumerable.Empty<UserSession>());
        _mockModelFactory.Setup(f => f.Create<UserSession>(InstanceNamesEnum.UserSession, _user))
            .Returns((_session, string.Empty));
        _mockModelFactory.Setup(f => f.Create<RefreshToken>(InstanceNamesEnum.RefreshToken, _user))
            .Returns((_refreshToken, rawToken));
        _mockRepositoryFactory.Setup(r => r.AddDependencyAsync(InstanceNamesEnum.UserSession, _session))
            .Returns(Task.CompletedTask);
        _mockRepositoryFactory.Setup(r => r.AddDependencyAsync(InstanceNamesEnum.RefreshToken, _refreshToken))
            .Returns(Task.CompletedTask);
        _mockJwtService.Setup(j => j.GenerateJwtSymmetricToken(It.IsAny<IEnumerable<System.Security.Claims.Claim>>()))
            .Returns("jwt-token");

        var result = await _sessionService.GenerateNewSessionAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("jwt-token", result.JwtToken);
        Assert.Equal(rawToken, result.RefreshToken);
        _mockRepositoryFactory.Verify(r => r.AddDependencyAsync(InstanceNamesEnum.UserSession, _session), Times.Once);
        _mockRepositoryFactory.Verify(r => r.AddDependencyAsync(InstanceNamesEnum.RefreshToken, _refreshToken), Times.Once);
    }

    [Fact]
    public async Task GenerateNewSessionAsync_WithCorruptedSession_ShouldRevokeAndThrow()
    {
        var dto = new LoginDto { Email = "test@example.com", Password = "SecurePass123!" };
        var corruptedSession = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            User = _user,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var corruptedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            TokenHash = "bad-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            UserSessionId = corruptedSession.Id
        };
        corruptedSession.RefreshTokens = new HashSet<RefreshToken> { corruptedToken };

        _mockLoginService.Setup(s => s.LoginUserAsync(dto)).ReturnsAsync(_user);
        _mockValidator.Setup(v => v.ValidateRegistration(ValidationField.Session, _user.Id.ToString()))
            .ReturnsAsync(ValidationResult.Failure([$"[{TokenReasonsEnum.Corrupted}] : Multiple active tokens"]));
        _mockRepositoryFactory.Setup(r => r.GetMultipleDependenciesAsync<UserSession>(
            InstanceNamesEnum.UserSession, _user.Id))
            .ReturnsAsync(new[] { corruptedSession });
        _mockRepositoryFactory.Setup(r => r.GetMultipleDependenciesAsync<RefreshToken>(
            InstanceNamesEnum.RefreshToken, corruptedSession.Id))
            .ReturnsAsync(new[] { corruptedToken });
        _mockRepositoryFactory.Setup(r => r.UpdateDependencyAsync(InstanceNamesEnum.RefreshToken, corruptedToken))
            .ReturnsAsync(true);
        _mockRepositoryFactory.Setup(r => r.UpdateDependencyAsync(InstanceNamesEnum.UserSession, corruptedSession))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<SessionException>(() => _sessionService.GenerateNewSessionAsync(dto));

        _mockRepositoryFactory.Verify(r => r.UpdateDependencyAsync(InstanceNamesEnum.UserSession, corruptedSession), Times.Once);
    }

    [Fact]
    public async Task GenerateNewSessionAsync_WhenLoginFails_ShouldThrow()
    {
        var dto = new LoginDto { Email = "test@example.com", Password = "WrongPass!" };

        _mockLoginService.Setup(s => s.LoginUserAsync(dto))
            .ThrowsAsync(new InvalidCredentialsException("Invalid credentials"));

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sessionService.GenerateNewSessionAsync(dto));

        _mockRepositoryFactory.Verify(r => r.AddDependencyAsync(
            It.IsAny<InstanceNamesEnum>(), It.IsAny<IUserDependency>()), Times.Never);
    }
}
