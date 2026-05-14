using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.ValidatorInterfaces;
using VictoriaIdentityProvider.Application.Services.Factory;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Validators;

public class RefreshTokenValidator : IAsyncValidator<string>
{
  private readonly  IRefreshTokenRepository _refreshTokenRepository;

    public RefreshTokenValidator
        (
        IRefreshTokenRepository refereshTokenRepository

        )
    {
        _refreshTokenRepository = refereshTokenRepository;
        
    }
    public async Task<ValidationResult> Validate(string  refreshToken)
    {
        var errors = new List<string>();

        

        var refreshStored =
           await _refreshTokenRepository
            .GetByRawTokenAsync(refreshToken);
            
        if (refreshStored is null) 
            return ValidationResult.Failure([$"[{TokenReasonsEnum.Corrupted}] : Refresh token not found."]);
       
        if (IsCompromised(refreshStored)) 
            errors.Add($"[{TokenReasonsEnum.Corrupted}] : Invalid Token.");
       
        if (refreshStored.IsExpired)
            
            errors.Add($"[{TokenReasonsEnum.Expired}] : Token expired");
        
        if (refreshStored.RevokedReason == RevokedReasonEnum.Logout)

            errors.Add($"[{TokenReasonsEnum.Logout}] : Token invalid due user logout");

      return errors.Any() 
            ? ValidationResult.Failure(errors) 
            : ValidationResult.Success();

      

    }

    private bool IsCompromised (RefreshToken token) 
    {
        if (token.ReplacedByTokenId.HasValue) 
            
            return true;

        if (token.RevokedReason == RevokedReasonEnum.Admin)

            return true;

        if (token.RevokedReason == RevokedReasonEnum.Corrupted)
            
            return true;
       
        if (token.UserId == Guid.Empty) 
            
            return true;

        return false;
    }
}
