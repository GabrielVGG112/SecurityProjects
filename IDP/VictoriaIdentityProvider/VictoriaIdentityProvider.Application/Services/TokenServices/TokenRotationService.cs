using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Enums;
using VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces;
using VictoriaIdentityProvider.Application.Services.Factory;
using VictoriaIdentityProvider.Application.Services.OrchestrationServices;
using VictoriaIdentityProvider.Domain.CustomErrors;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Services.TokenServices
{
    public class TokenRotationService : ITokenRotationService
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ITokenFactory _tokenFactory;
        private readonly HeaderValidatorFacade _asyncValidatorFacade;

        public TokenRotationService(
            IRepositoryFactory repositoryFactory,
            ITokenFactory tokenFactory,
            HeaderValidatorFacade asyncValidatorFacade
            )
        {
            _repositoryFactory = repositoryFactory;
 
            _asyncValidatorFacade = asyncValidatorFacade;
            _tokenFactory= tokenFactory;
      
        }


        public async Task<TokenResultDto> RotateTokensAsync(TokenResultDto dto)
        {

           
            ValidationResult result = await _asyncValidatorFacade.ValidateRegistration(dto);
            RefreshToken storedRefreshToken =  await _repositoryFactory
                .GetDependencyByTokenAsync<RefreshToken>(InstanceNamesEnum.RefreshToken, dto.RefreshToken);
          
            UserSession session = storedRefreshToken?.UserSession
                ?? throw new SessionException("Invalid session");

            await MakeDecision(result, storedRefreshToken!, session);
            if (session.IsExpired)
            {
                session.RevokedAt = DateTime.UtcNow;
                session.RevokedReason = RevokedReasonEnum.Expired;
                bool succeded = await _repositoryFactory.UpdateDependencyAsync(InstanceNamesEnum.UserSession, session);
          
                if (!succeded) throw new SessionConcurrencyException($"Update failed  Session : {session.Id}");
                throw new SessionException("Session Expired");  
            }
            // throws RefreshTokenException() 
            
            var tokenResult = await GetBundleTokensAsync(storedRefreshToken!, session);
            return tokenResult;
        }

        private async Task MakeDecision(
            ValidationResult result,
            RefreshToken storedRefreshToken,
            UserSession session)
        {
            bool isCompromised = result.Errors.Any(e => e.Contains($"[{TokenReasonsEnum.Corrupted}]"));

            if (isCompromised)
            {

                session.RevokedAt = DateTime.UtcNow;
                session.RevokedReason = RevokedReasonEnum.Corrupted;

                storedRefreshToken.RevokedAt = DateTime.UtcNow;
                storedRefreshToken.RevokedReason = RevokedReasonEnum.Corrupted;
                bool succeded = await _repositoryFactory.UpdateDependencyAsync(InstanceNamesEnum.UserSession, session);
               
                if (!succeded) throw new SessionConcurrencyException("Session update failed");
              
                succeded = await _repositoryFactory.UpdateDependencyAsync<RefreshToken>(InstanceNamesEnum.RefreshToken, storedRefreshToken!);
                
                if (!succeded) throw new RefreshTokenConcurencyException(["Update failed"]);
                throw new RefreshTokenException(result.Errors);
            }

            if (!result.IsValid && result.Errors.Any(e => 
            e.Contains($"[{TokenReasonsEnum.Expired}]") &&
            e.Contains(ValidationField.RefreshToken.ToString())))
            {
                session.RevokedAt = DateTime.UtcNow;
                session.RevokedReason = RevokedReasonEnum.Expired;

              bool succeded = await _repositoryFactory.UpdateDependencyAsync(InstanceNamesEnum.UserSession, session);
                if (!succeded) throw new SessionConcurrencyException($"Update failed Session : {session.Id}");
                throw new RefreshTokenException(result.Errors);
            }
        }

        private async Task<TokenResultDto> GetBundleTokensAsync(RefreshToken storedRefreshToken, UserSession session)
        {
         
          var (newRefresh, rawToken) =  _tokenFactory.CreateModel(session.User);

            storedRefreshToken.ReplacedByTokenId = newRefresh.Id;
            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            storedRefreshToken.RevokedReason = RevokedReasonEnum.Rotation;
            newRefresh.UserSessionId = session.Id;
            newRefresh.UserSession = session;
            session.RefreshTokenId = newRefresh.Id;
            session.LastActivityAt = DateTime.UtcNow;
            bool succeded = await _repositoryFactory.UpdateDependencyAsync(InstanceNamesEnum.UserSession, session);
           
            if (!succeded) 
                throw new SessionConcurrencyException($"Session update failed: {session.Id}");
           
            succeded = await _repositoryFactory.UpdateDependencyAsync(InstanceNamesEnum.RefreshToken, storedRefreshToken);


            if (!succeded) throw new RefreshTokenConcurencyException(["Token update failed"]);

            await _repositoryFactory.AddDependencyAsync(InstanceNamesEnum.RefreshToken, newRefresh);
            
           
    


            var jwt = _tokenFactory.GenerateJwtToken(session.User, session.Id);


            return new TokenResultDto 
            { 
              JwtToken = jwt,
              RefreshToken= rawToken,
              ValidUntil = newRefresh.ExpiresAt
            };
                
        }


    }
}


