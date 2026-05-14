using System.Security.Claims;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Enums;
using VictoriaIdentityProvider.Application.Interfaces;
using VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;
using VictoriaIdentityProvider.Application.Services.OrchestrationServices;
using VictoriaIdentityProvider.Domain.CustomErrors;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Services.ClientServices
{
    public class LogoutService : ILogoutService
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IJwtAccessTokenService _jwtService;
        private readonly HeaderValidatorFacade _headerValidatorFacade;
        private readonly IUserService _userService;
        
        public LogoutService(
            IRepositoryFactory repositoryFactory,
            IJwtAccessTokenService jwtService,
            HeaderValidatorFacade headerValidatorFacade,
            IUserService userService)
        
        {
            _repositoryFactory = repositoryFactory;
            _jwtService = jwtService;
            _headerValidatorFacade = headerValidatorFacade;
            _userService = userService;
        }

        public async Task LogoutUserAsync(TokenResultDto dto)
        {
          
          var result = await _headerValidatorFacade
                .ValidateRegistration(dto);
           
            if (!result.IsValid )
            {
                throw new SessionException(result.Errors);
            }
           
            ClaimsPrincipal principal = _jwtService.ValidateJwtSymmetricToken(dto.JwtToken) 
                ?? throw new SessionException("session corrupted");
            
            string sessionIdValue = GetClaimFromPrincipal(principal,CustomClaimTypes.SessionId);
            
           
            Guid sessionId = GetIdFromClaim(sessionIdValue);

            RefreshToken refreshToken = await _repositoryFactory
                .GetDependencyByTokenAsync<RefreshToken>(InstanceNamesEnum.RefreshToken, dto.RefreshToken);

            UserSession session = await _repositoryFactory
                .GetDependencyByIdAsync<UserSession>(InstanceNamesEnum.UserSession, sessionId);
           
            result = await _headerValidatorFacade
                .ValidateRegistration(ValidationField.Session, session.Id.ToString());
           
            if (!result.IsValid) 
            {
                foreach (RefreshToken token in session.RefreshTokens) 
                {
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedReason = RevokedReasonEnum.Corrupted;
                    await _repositoryFactory.UpdateDependencyAsync(InstanceNamesEnum.RefreshToken, token);
                   
                }
                session.RevokedAt = DateTime.UtcNow;
                session.LastActivityAt = DateTime.UtcNow;
                session.RevokedReason = RevokedReasonEnum.Corrupted;
                await _repositoryFactory.UpdateDependencyAsync(InstanceNamesEnum.UserSession, session);
                throw new SessionException("Session was corrupted and has been revoked and all related tokens have been invalidated.");
            }

           User user = await _userService.GetUserByIdAsync(session.UserId);

           session.RevokedAt = DateTime.UtcNow;
           session.LastActivityAt = DateTime.UtcNow;
           session.RevokedReason = RevokedReasonEnum.Logout;
          
           refreshToken.RevokedAt = DateTime.UtcNow;
           refreshToken.RevokedReason = RevokedReasonEnum.Logout;
           refreshToken.ReplacedByTokenId = null;
           
            
            await _repositoryFactory.UpdateDependencyAsync(InstanceNamesEnum.RefreshToken, refreshToken);
            await _repositoryFactory.UpdateDependencyAsync(InstanceNamesEnum.UserSession, session);

            IEnumerable<UserSession> sessions = await _repositoryFactory
                .GetMultipleDependenciesAsync<UserSession>(InstanceNamesEnum.UserSession, session.UserId);
           
            user.LastLogInAt = DateTime.UtcNow;
            
            if (!sessions.Any())
            {
                user.UserStatus = UserStatusEnum.Inactive;

            }
            await _userService.UpdateUserAsync(user);
        }

        public async Task LogoutFromAllDevicesAsync(TokenResultDto dto)
        {
            var result = await _headerValidatorFacade.ValidateRegistration(dto);
            if (!result.IsValid)
            {
                throw new SessionException(result.Errors);
            }
            ClaimsPrincipal principal = _jwtService.ValidateJwtSymmetricToken(dto.JwtToken)
                ?? throw new SessionException("session corrupted");

            string userIdValue = GetClaimFromPrincipal(principal, ClaimTypes.NameIdentifier);
           
            var user = await _userService
                .GetUserByIdAsync(GetIdFromClaim(userIdValue));
           
            var sessions = await _repositoryFactory
                .GetMultipleDependenciesAsync<UserSession>(InstanceNamesEnum.UserSession, user.Id);
            
            await InvalidateAllSessions(sessions); 

        }

        private async Task InvalidateAllSessions(IEnumerable<UserSession> sessions) 
        {
            foreach(var session in sessions) 
            {
                var result =await _headerValidatorFacade.ValidateRegistration(ValidationField.Session, session.Id.ToString());
                if (!result.IsValid) throw new SessionException("Invalid session");
                
                Guid rTokenId = session.RefreshTokenId ?? throw new SessionException("Token cant be null");
               
                var token = await _repositoryFactory.GetDependencyByIdAsync<RefreshToken>(InstanceNamesEnum.RefreshToken, rTokenId);

               await  InvalidateRefreshTokensAsync(token);
                session.RevokedAt = DateTime.UtcNow;
                session.RevokedReason = RevokedReasonEnum.Logout;
                session.LastActivityAt = DateTime.UtcNow;
                await _repositoryFactory.UpdateDependencyAsync(InstanceNamesEnum.UserSession,session);
            }
        }
        private async Task InvalidateRefreshTokensAsync(RefreshToken token) 
        {
            if (token.IsActive) 
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = RevokedReasonEnum.Logout;
                token.ReplacedByTokenId = null;
                await _repositoryFactory.UpdateDependencyAsync(InstanceNamesEnum.RefreshToken, token);
            }
        }
        private string GetClaimFromPrincipal(ClaimsPrincipal principal, string claimType)
        {

            var claim = principal.Claims.SingleOrDefault(c => c.Type == claimType)
                ??
                throw new ClaimException("Invalid Token");

            return claim.Value;
        }
        private Guid GetIdFromClaim(string claim)
        {
            if (!Guid.TryParse(claim, out var id)) throw new SessionException("Invalid session");

            return id;
        }
    }
}
