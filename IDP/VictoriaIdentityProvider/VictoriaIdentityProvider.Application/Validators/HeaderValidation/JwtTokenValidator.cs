using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.ValidatorInterfaces;
using VictoriaIdentityProvider.Application.Services.Factory;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Validators.HeaderValidation
{
    public class JwtTokenValidator : IAsyncValidator<string>
    {
        IJwtAccessTokenService _jwtService;
        ISessionRepository _sessionRepo;
        IUserService _userService;


        public JwtTokenValidator(
            IJwtAccessTokenService jwtSevice,
            ISessionRepository sessionRepo,IUserService userService)
        {
            _jwtService = jwtSevice;
            _sessionRepo= sessionRepo;
            _userService = userService;
        }

        public async Task<ValidationResult> Validate(string jwtToken)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(jwtToken))
                return ValidationResult.Failure([$"[{TokenReasonsEnum.Corrupted}] : invalid token"]);

            ClaimsPrincipal? principal = null;
            try
            {
                principal = _jwtService.ValidateJwtSymmetricToken(jwtToken);
              
                if (principal is null)
                    return ValidationResult.Failure([$"[{TokenReasonsEnum.Corrupted}] :  Invalid Token"]);
            }
            catch (SecurityTokenExpiredException)
            {
               
                errors.Add($"[{TokenReasonsEnum.Expired}] :  Token is expired"); 
            }
            catch (Exception)
            {
                errors.Add($"[{TokenReasonsEnum.Corrupted}] :  Invalid Token");

            }

           
            Claim?  sessionIdClaim  = principal?.Claims
                .SingleOrDefault(c => c.Type == CustomClaimTypes.SessionId);

            if (sessionIdClaim is null)
                return ValidationResult.Failure([$"[{TokenReasonsEnum.Corrupted}] :  Invalid Token"]);

            Guid? sessionId = GetUserFromTokenValue(sessionIdClaim.Value);
            
            if ( sessionId is null) 
               return ValidationResult.Failure([$"[{TokenReasonsEnum.Corrupted}] :  Invalid Token"]);

            var session = await _sessionRepo.GetUserSessionById(sessionId.Value);

            if(session is null) 
                return ValidationResult.Failure([$"[{TokenReasonsEnum.Corrupted}] :  Invalid Token"]);
            var user = await _userService.GetUserByIdAsync(session.UserId);
            ValidateClaims(ref errors, principal!.Claims, session, user);

             
            return errors.Any() 
                ? ValidationResult.Failure(errors)
                : ValidationResult.Success();
        }
        // Helper method to extract user information from the token's claim value
        private  Guid? GetUserFromTokenValue(string claimValue)
        {
            if (!Guid.TryParse(claimValue, out var result)) return null;



            return  result;
        }
        // Helper method to validate the claims against the expected values from the user session
        // i have to keep an eye on it because i need to  check if its beter with lower security or not 
        private void ValidateClaims(ref List<string> errors, IEnumerable<Claim> claims,UserSession session,User user)
        {
            var strictClaims = new Dictionary<string, string>()
        {
            { ClaimTypes.NameIdentifier, user.Id.ToString() },
            { CustomClaimTypes.SessionId, session.Id.ToString() } 
            //{ ClaimTypes.Email, user.Email },
            //{ ClaimTypes.GivenName, user.FirstName},
            //{ ClaimTypes.Surname, user.LastName },
            //{ ClaimTypes.Name, user.FullName },
          
            //{ CustomClaimTypes.EmailVerified, user.IsEmailConfirmed ? "true" : "false" },
            //{ CustomClaimTypes.PreferredUsername, user.Email },

          
        };
            var requiredClaims = new HashSet<string>
            {
                ClaimTypes.Email,
                ClaimTypes.GivenName,
                ClaimTypes.Surname,
                ClaimTypes.Name,
                CustomClaimTypes.EmailVerified,
                CustomClaimTypes.PreferredUsername
            };

            foreach (var strictlyRequired in strictClaims.Keys)
            {
                if (!claims.Any(c => c.Type == strictlyRequired))
                    errors.Add($"[{TokenReasonsEnum.Corrupted}] : Missing claim {strictlyRequired}");
            }
           
            foreach (var claim in claims)
            {
                if (!strictClaims.TryGetValue(claim.Type, out var expected))
                    continue;

                if (claim.Type == CustomClaimTypes.EmailVerified)
                {
                    if (!string.Equals(claim.Value, expected, StringComparison.OrdinalIgnoreCase))
                        errors.Add($"[{TokenReasonsEnum.Corrupted}] : Invalid claim {claim.Type}");
                }
                else
                {
                    if (claim.Value != expected)
                        errors.Add($"[{TokenReasonsEnum.Corrupted}] : Invalid claim {claim.Type}");
                }
            }

            foreach (var type in requiredClaims)
            {
                if (!claims.Any(c => c.Type == type))
                    errors.Add($"[{TokenReasonsEnum.Corrupted}] : Missing claim {type}");
            }
        }
    }
}
