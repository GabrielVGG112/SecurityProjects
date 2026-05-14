using Moq;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Enums;
using VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces;
using VictoriaIdentityProvider.Application.Services.Factory;
using VictoriaIdentityProvider.Application.Services.OrchestrationServices;
using VictoriaIdentityProvider.Application.Services.TokenServices;
using VictoriaIdentityProvider.Application.Validators;
using VictoriaIdentityProvider.Domain.CustomErrors;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Tests.NewTests
{
    public class TokenRotationTests
    {
        private readonly TokenRotationService _rotationService;
        private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
        private readonly Mock<ITokenFactory> _mockTokenFactory;
        private readonly Mock<HeaderValidatorFacade> _mockValidatorFacade;

        private readonly TokenResultDto _tokenResultDto;
        private readonly User _user;
        private readonly UserSession _session;
        private readonly RefreshToken _storedToken;
        private readonly RefreshToken _newToken;

        public TokenRotationTests()
        {
            _mockRepositoryFactory = new Mock<IRepositoryFactory>();
            _mockTokenFactory = new Mock<ITokenFactory>();
            _mockValidatorFacade = new Mock<HeaderValidatorFacade>();

            _rotationService = new TokenRotationService(
                _mockRepositoryFactory.Object,
                _mockTokenFactory.Object,
                _mockValidatorFacade.Object);

            _user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
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

            _session.RefreshTokenId = _storedToken.Id;

            _newToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = _user.Id,
                TokenHash = "new-hashed-token",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _tokenResultDto = new TokenResultDto
            {
                JwtToken = "jwt-token",
                RefreshToken = "raw-token",
                ValidUntil = DateTime.UtcNow.AddDays(7)
            };
        }

        [Fact]
        public async Task RotateTokensAsync_WithValidTokens_ShouldReturnNewTokenBundle()
        {
            _mockValidatorFacade.Setup(v => v.ValidateRegistration(_tokenResultDto))
                .ReturnsAsync(ValidationResult.Success());
            _mockRepositoryFactory.Setup(r => r.GetDependencyByTokenAsync<RefreshToken>(
                InstanceNamesEnum.RefreshToken, _tokenResultDto.RefreshToken))
                .ReturnsAsync(_storedToken);
            _mockTokenFactory.Setup(f => f.CreateModel(_user))
                .Returns((_newToken, "new-raw-token"));
            _mockTokenFactory.Setup(f => f.GenerateJwtToken(_user, _session.Id))
                .Returns("new-jwt-token");
            _mockRepositoryFactory.Setup(r => r.UpdateDependencyAsync(InstanceNamesEnum.UserSession, _session))
                .ReturnsAsync(true);
            _mockRepositoryFactory.Setup(r => r.UpdateDependencyAsync(InstanceNamesEnum.RefreshToken, _storedToken))
                .ReturnsAsync(true);
            _mockRepositoryFactory.Setup(r => r.AddDependencyAsync(InstanceNamesEnum.RefreshToken, _newToken))
                .Returns(Task.CompletedTask);

            var result = await _rotationService.RotateTokensAsync(_tokenResultDto);

            Assert.NotNull(result);
            Assert.Equal("new-jwt-token", result.JwtToken);
            Assert.Equal("new-raw-token", result.RefreshToken);
            Assert.Equal(RevokedReasonEnum.Rotation, _storedToken.RevokedReason);
            Assert.Equal(_newToken.Id, _storedToken.ReplacedByTokenId);
        }

        [Fact]
        public async Task RotateTokensAsync_WithCompromisedToken_ShouldRevokeAndThrow()
        {
            _mockValidatorFacade.Setup(v => v.ValidateRegistration(_tokenResultDto))
                .ReturnsAsync(ValidationResult.Failure([$"[{TokenReasonsEnum.Corrupted}] : Token compromised"]));
            _mockRepositoryFactory.Setup(r => r.GetDependencyByTokenAsync<RefreshToken>(
                InstanceNamesEnum.RefreshToken, _tokenResultDto.RefreshToken))
                .ReturnsAsync(_storedToken);
            _mockRepositoryFactory.Setup(r => r.UpdateDependencyAsync(InstanceNamesEnum.UserSession, _session))
                .ReturnsAsync(true);
            _mockRepositoryFactory.Setup(r => r.UpdateDependencyAsync(InstanceNamesEnum.RefreshToken, _storedToken))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<RefreshTokenException>(() => _rotationService.RotateTokensAsync(_tokenResultDto));

            Assert.Equal(RevokedReasonEnum.Corrupted, _storedToken.RevokedReason);
            Assert.Equal(RevokedReasonEnum.Corrupted, _session.RevokedReason);
        }

        [Fact]
        public async Task RotateTokensAsync_WithExpiredToken_ShouldRevokeSessionAndThrow()
        {
            _mockValidatorFacade.Setup(v => v.ValidateRegistration(_tokenResultDto))
                .ReturnsAsync(ValidationResult.Failure([$"[{TokenReasonsEnum.Expired}] : {ValidationField.RefreshToken} expired"]));
            _mockRepositoryFactory.Setup(r => r.GetDependencyByTokenAsync<RefreshToken>(
                InstanceNamesEnum.RefreshToken, _tokenResultDto.RefreshToken))
                .ReturnsAsync(_storedToken);
            _mockRepositoryFactory.Setup(r => r.UpdateDependencyAsync(InstanceNamesEnum.UserSession, _session))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<RefreshTokenException>(() => _rotationService.RotateTokensAsync(_tokenResultDto));

            Assert.Equal(RevokedReasonEnum.Expired, _session.RevokedReason);
        }

        [Fact]
        public async Task RotateTokensAsync_WithExpiredSession_ShouldRevokeSessionAndThrow()
        {
            _session.ExpiresAt = DateTime.UtcNow.AddDays(-1);

            _mockValidatorFacade.Setup(v => v.ValidateRegistration(_tokenResultDto))
                .ReturnsAsync(ValidationResult.Success());
            _mockRepositoryFactory.Setup(r => r.GetDependencyByTokenAsync<RefreshToken>(
                InstanceNamesEnum.RefreshToken, _tokenResultDto.RefreshToken))
                .ReturnsAsync(_storedToken);
            _mockRepositoryFactory.Setup(r => r.UpdateDependencyAsync(InstanceNamesEnum.UserSession, _session))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<SessionException>(() => _rotationService.RotateTokensAsync(_tokenResultDto));

            Assert.Equal(RevokedReasonEnum.Expired, _session.RevokedReason);
        }
    }
}


