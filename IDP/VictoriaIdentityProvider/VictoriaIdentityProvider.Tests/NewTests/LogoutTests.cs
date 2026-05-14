using Moq;
using System.Security.Claims;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Enums;
using VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;
using VictoriaIdentityProvider.Application.Services.ClientServices;
using VictoriaIdentityProvider.Application.Services.Factory;
using VictoriaIdentityProvider.Application.Services.OrchestrationServices;
using VictoriaIdentityProvider.Domain.CustomErrors;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Tests.NewTests
{
    public class LogoutTests
    {
        private User _user;
        private UserSession _session;

        private RefreshToken _storedToken;
        private RefreshToken _storedToken2;
        private RefreshToken _storedToken3;
        private RefreshToken _storedToken4;
        
        private TokenResultDto _tokenResult;

        private Claim[] _claims;
        private ClaimsPrincipal _principal;
        private readonly Mock<IRepositoryFactory> _mockFactory;
        private readonly Mock<IJwtAccessTokenService> _mockJwtService;
        private readonly Mock<HeaderValidatorFacade> _mockValidator;
        private readonly Mock<IUserService> _mockUserService;



        private readonly LogoutService _logoutService;


        public LogoutTests()
        {
            _mockFactory = new Mock<IRepositoryFactory>();
            _mockJwtService = new Mock<IJwtAccessTokenService>();
            _mockValidator = new Mock<HeaderValidatorFacade>();
            _mockUserService = new Mock<IUserService>();


            _logoutService = new LogoutService(_mockFactory.Object,
                _mockJwtService.Object,
                _mockValidator.Object,
                _mockUserService.Object
                );

            _user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                LastLogInAt = DateTime.UtcNow.AddMinutes(-5)
            };

            _session = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = _user.Id,
                User = _user,
                SessionToken = "hashed-session-token",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _storedToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = _user.Id,
                User = _user,
                TokenHash = "hashed-token",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserSessionId = _session.Id,
                UserSession = _session
            };

            _storedToken2 = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = _user.Id,
                User = _user,
                TokenHash = "hashed-token",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserSessionId = _session.Id,
                UserSession = _session,
                RevokedReason = RevokedReasonEnum.Logout,
                RevokedAt = DateTime.UtcNow.AddDays(-2)
            };
           _storedToken3 = new RefreshToken
           {
               Id = Guid.NewGuid(),
               UserId = _user.Id,
               User = _user,
               TokenHash = "hashed-token",
               ExpiresAt = DateTime.UtcNow.AddDays(7),
               UserSessionId = _session.Id,
               UserSession = _session,
               RevokedReason = RevokedReasonEnum.Rotation,
               RevokedAt = DateTime.UtcNow.AddDays(-4)
           };
            _storedToken4 = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = _user.Id,
                User = _user,
                TokenHash = "hashed-token",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserSessionId = _session.Id,
                UserSession = _session,
                ReplacedByTokenId = _storedToken3.Id,
                RevokedReason = RevokedReasonEnum.Rotation,
                RevokedAt = DateTime.UtcNow.AddDays(-5)
            };


            _claims = DefaultJwtClaimsFactory.BuildClaimsForUser(_user, _session.Id);
            _principal = new ClaimsPrincipal(new ClaimsIdentity(_claims));
            _session.RefreshTokens.Add(_storedToken);
         
            _tokenResult = new TokenResultDto
            {
                JwtToken = "valid-jwt-token",
                RefreshToken = "valid-refresh-token",
                ValidUntil = DateTime.UtcNow.AddDays(7)
            };


        }

        [Fact]
        public async Task LogoutUserAsync_WithRightInput_ShoudReturnTrue()
        {
            _mockValidator
              .Setup(v => v.ValidateRegistration(_tokenResult))
              .ReturnsAsync(ValidationResult.Success());

            _mockJwtService
                .Setup(v => v.ValidateJwtSymmetricToken(_tokenResult.JwtToken))
                .Returns(_principal);

            _mockFactory.Setup(f => f.GetDependencyByTokenAsync<RefreshToken>(
                InstanceNamesEnum.RefreshToken,
                _tokenResult.RefreshToken))
                .ReturnsAsync(_storedToken);

            _mockFactory.Setup(f => f.GetDependencyByIdAsync<UserSession>(
                InstanceNamesEnum.UserSession,
                _session.Id))
                .ReturnsAsync(_session);

            _mockValidator.Setup(v => v.ValidateRegistration(
                ValidationField.Session,
                _session.Id.ToString()))
                .ReturnsAsync(ValidationResult.Success());

            _mockFactory.Setup(mf => mf.UpdateDependencyAsync(InstanceNamesEnum.RefreshToken, _storedToken))
                .ReturnsAsync(true);
            _mockFactory.Setup(mf => mf.UpdateDependencyAsync(InstanceNamesEnum.UserSession, _session))
             .ReturnsAsync(true);

            _mockUserService.Setup(us => us.GetUserByIdAsync(_session.UserId))
                    .ReturnsAsync(_user);

            _mockFactory.Setup(mf => mf.GetMultipleDependenciesAsync<UserSession>
            (InstanceNamesEnum.UserSession, _user.Id))
                .ReturnsAsync(Enumerable.Empty<UserSession>);

            _mockUserService.Setup(us => us.UpdateUserAsync(_user)).Returns(Task.CompletedTask);



            await _logoutService.LogoutUserAsync(_tokenResult);


            Assert.Equal(RevokedReasonEnum.Logout, _session.RevokedReason);
            Assert.Equal(RevokedReasonEnum.Logout, _storedToken.RevokedReason);

            Assert.NotNull(_session.RevokedAt);
            Assert.True((_session.RevokedAt.Value - DateTime.UtcNow).Duration() < TimeSpan.FromSeconds(2));

            Assert.NotNull(_storedToken.RevokedAt);
            Assert.True((_storedToken.RevokedAt.Value - DateTime.UtcNow).Duration() < TimeSpan.FromSeconds(2));

            Assert.True((_user.LastLogInAt!.Value - DateTime.UtcNow).Duration() < TimeSpan.FromSeconds(2));

        }

        [Fact]

        public async Task LogoutUserAsync_WithWrongInput_ShoudThrowSessionException()
        {
            _mockValidator
              .Setup(v => v.ValidateRegistration(_tokenResult))
              .ReturnsAsync(ValidationResult.Failure("Invalid input"));
            await Assert.ThrowsAsync<SessionException>(() => _logoutService.LogoutUserAsync(_tokenResult));
        }
        [Fact]
        public async Task LogoutUserAsync_WithCorruptedSession_ShoudRevokeSessionAndThrowSessionException()
        {
            _session.RefreshTokens.Add(_storedToken2);
            _session.RefreshTokens.Add(_storedToken3);
            _session.RefreshTokens.Add(_storedToken4);

            _mockValidator
                .Setup(v => v.ValidateRegistration(_tokenResult))
                .ReturnsAsync(ValidationResult.Success());

            _mockJwtService
                .Setup(v => v.ValidateJwtSymmetricToken(_tokenResult.JwtToken))
                .Returns(_principal);

            _mockFactory.Setup(f => f.GetDependencyByTokenAsync<RefreshToken>(
                InstanceNamesEnum.RefreshToken,
                _tokenResult.RefreshToken))
                .ReturnsAsync(_storedToken);

            _mockFactory.Setup(f => f.GetDependencyByIdAsync<UserSession>(
                InstanceNamesEnum.UserSession,
                _session.Id))
                .ReturnsAsync(_session);

            _mockValidator.Setup(v => v.ValidateRegistration(
                ValidationField.Session,
                _session.Id.ToString()))
                .ReturnsAsync(ValidationResult.Failure("Invalid session"));


            await Assert.ThrowsAsync<SessionException>(() => _logoutService.LogoutUserAsync(_tokenResult));

#pragma warning disable CS8629 
            
            Assert.Equal(RevokedReasonEnum.Corrupted, _storedToken.RevokedReason);
            Assert.True((_storedToken.RevokedAt.Value - DateTime.UtcNow).Duration() < TimeSpan.FromSeconds(2));
            Assert.Equal(RevokedReasonEnum.Corrupted, _storedToken2.RevokedReason);
            Assert.True((_storedToken2.RevokedAt.Value - DateTime.UtcNow).Duration() < TimeSpan.FromSeconds(2));
            Assert.Equal(RevokedReasonEnum.Corrupted, _storedToken2.RevokedReason);
            Assert.True((_storedToken3.RevokedAt.Value - DateTime.UtcNow).Duration() < TimeSpan.FromSeconds(2));
            Assert.Equal(RevokedReasonEnum.Corrupted, _storedToken3.RevokedReason);
            Assert.True((_storedToken4.RevokedAt.Value - DateTime.UtcNow).Duration() < TimeSpan.FromSeconds(2));
            Assert.Equal(RevokedReasonEnum.Corrupted, _storedToken4.RevokedReason);
            
            Assert.Equal(RevokedReasonEnum.Corrupted, _session.RevokedReason);
            Assert.NotNull(_session.RevokedAt);
            Assert.True((_session.RevokedAt.Value - DateTime.UtcNow).Duration() < TimeSpan.FromSeconds(2));
        }
    }
}