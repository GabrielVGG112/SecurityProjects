using VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.ValidatorInterfaces;
using VictoriaIdentityProvider.Application.Services.Factory;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Validators.HeaderValidation
{
    public class SessionValidator : IAsyncValidator<string>
    {
        private readonly IClientContextService _clientContext;
        private readonly ISessionRepository _sessionRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        

        public SessionValidator(
            IClientContextService clientContext,
            ISessionRepository sessionRepository,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _clientContext = clientContext;
            _sessionRepository = sessionRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<ValidationResult> Validate(string userId)
        {
            var errors = new List<string>();
            if (! Guid.TryParse(userId ,out var result)) 
            {
                return ValidationResult.Failure([$"[{TokenReasonsEnum.Corrupted}] : Invalid User Id"]);
            }
           
    
            var meta = _clientContext.GetClientMetadata();
            var sessions =await _sessionRepository.GetActiveUserSessionAsync(result);

            if (sessions.Count(s => s.DeviceId == meta.DeviceId) > 1)
            {
                return ValidationResult.Failure([$"[{TokenReasonsEnum.Corrupted}] : Multiple Sessions with same device id."]);
            }
           
            UserSession? session =
            sessions.SingleOrDefault(s => s.DeviceId == meta.DeviceId);

            if (session is not null) 
            {
                IEnumerable<RefreshToken> rt = await  _refreshTokenRepository.GetTokensBySessionId(session.Id);
             
                RefreshToken[] refreshTokens = rt.Where(r=> r.IsActive).ToArray();

                if (refreshTokens.Length > 1) 
                {
                    errors.Add($"[{TokenReasonsEnum.Corrupted}] : Multiple active refresh tokens.");
                }
                if (refreshTokens.Length == 1) 
                {
                    var token = refreshTokens[0];
                     if (token.Id != session.RefreshTokenId) 
                    {
                        errors.Add($"[{TokenReasonsEnum.Corrupted}] : RefreshToken Is not for this session.");
                    }
                   
                } 
                if (refreshTokens.Length == 0) 
                {
                    RefreshToken? storedRefresh = rt.SingleOrDefault(r => r.Id == session.RefreshTokenId); 
                    if (storedRefresh is null) 
                        return ValidationResult.Failure([$"[{TokenReasonsEnum.Corrupted}] : No refresh tokens asociated with the session"]);
                    if (
                          storedRefresh.RevokedReason == RevokedReasonEnum.Admin 
                        ||storedRefresh.RevokedReason == RevokedReasonEnum.Rotation
                        ||storedRefresh.RevokedReason == RevokedReasonEnum.Corrupted
                        ||storedRefresh.RevokedReason == RevokedReasonEnum.Logout
                        ) 
                    {
                        errors.Add($"[{TokenReasonsEnum.Corrupted}] :This session contains a refreshToken allredy revoked.");
                    }

                    if (storedRefresh.IsExpired)
                        errors.Add($"[{TokenReasonsEnum.Expired}] : Session is expired.");

                }
                 
            }

            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        }
    }
}
