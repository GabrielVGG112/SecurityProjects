using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Enums;
using VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;
using VictoriaIdentityProvider.Application.Services.Factory;
using VictoriaIdentityProvider.Application.Services.OrchestrationServices;
using VictoriaIdentityProvider.Domain.CustomErrors;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Services.ClientServices;
    public class SessionService : ISessionService
{
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly IJwtAccessTokenService _jwtAccessTokenService;
    private readonly ILoginService _loginService;
    private readonly HeaderValidatorFacade _asyncValidator;
    private readonly ModelFactoryFacade _modelFactory;

    public SessionService
        (
        
        IRepositoryFactory repositoryFactory,
        IJwtAccessTokenService jwtAccessTokenService,
        ILoginService loginService,
        ModelFactoryFacade modelFactory,
        HeaderValidatorFacade asyncValidator
        )
    {
        _repositoryFactory = repositoryFactory;
        _jwtAccessTokenService = jwtAccessTokenService;
        _modelFactory = modelFactory;
        _loginService = loginService;
        _asyncValidator = asyncValidator;
    }

    public async Task<TokenResultDto> GenerateNewSessionAsync(LoginDto loginDto)
    {
        User user = await _loginService.LoginUserAsync(loginDto);
        await MakeRevocationDecision(user.Id);

        var (session, _) = _modelFactory.Create<UserSession>(InstanceNamesEnum.UserSession, user);
        var (refreshToken, rawToken) = _modelFactory.Create<RefreshToken>(InstanceNamesEnum.RefreshToken, user);

        session.RefreshTokenId = refreshToken.Id;
        refreshToken.UserSessionId = session.Id;
        refreshToken.ExpiresAt = session.ExpiresAt;
        await _repositoryFactory.AddDependencyAsync(InstanceNamesEnum.UserSession, session);
        await _repositoryFactory.AddDependencyAsync(InstanceNamesEnum.RefreshToken, refreshToken);
       

        var jwt = _jwtAccessTokenService.GenerateJwtSymmetricToken(
            DefaultJwtClaimsFactory.BuildClaimsForUser(user, session.Id));

        return new TokenResultDto
        {
            JwtToken = jwt,
            RefreshToken = rawToken,
            ValidUntil = refreshToken.ExpiresAt
        };
    }

    private async Task MakeRevocationDecision(Guid userId)
    {
        var result = await _asyncValidator.ValidateRegistration(ValidationField.Session, userId.ToString());
    
        var sessions = await _repositoryFactory.GetMultipleDependenciesAsync<UserSession>(InstanceNamesEnum.UserSession, userId);
        bool isCorrupted = result.Errors.Any(e => e.Contains($"[{TokenReasonsEnum.Corrupted}]"));

        if (!result.IsValid && (isCorrupted || sessions.Any()))
        {
            await RevokeAllUserSessionsAndTokensAsync(sessions);
            throw new SessionException("Multiple corrupted sessions found");
        }
    }

    private async Task RevokeAllUserSessionsAndTokensAsync(IEnumerable<UserSession> sessions)
    {
        foreach (var session in sessions)
        {
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedReason = RevokedReasonEnum.Corrupted;

            await RevokeSessionTokensAsync(session.RefreshTokenId.Value);

            bool succeeded = await _repositoryFactory.UpdateDependencyAsync(InstanceNamesEnum.UserSession,session);
            if (!succeeded)
                throw new SessionConcurrencyException($"Updating session {session.Id} failed");
        }
    }

    private async Task RevokeSessionTokensAsync(Guid refreshTokenId)
    {
        var token = await _repositoryFactory.GetDependencyByIdAsync<RefreshToken>(InstanceNamesEnum.RefreshToken, refreshTokenId);
       
        
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = RevokedReasonEnum.Corrupted;

            bool isValid = await _repositoryFactory.UpdateDependencyAsync(InstanceNamesEnum.RefreshToken, token);
            if (!isValid)
                throw new RefreshTokenConcurencyException([$"Failed to revoke token {token.Id}"]);
        }
    }






