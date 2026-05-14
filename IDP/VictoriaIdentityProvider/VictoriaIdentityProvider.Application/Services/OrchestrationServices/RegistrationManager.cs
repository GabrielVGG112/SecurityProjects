using Microsoft.IdentityModel.Tokens;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.OptionsInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;
using VictoriaIdentityProvider.Domain.CustomErrors;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Services.OrchestrationServices
{
    public class RegistrationManager :IRegistrationManager
    {
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;
        private readonly IJwtInternalToken _jwtInternalService;
        private readonly IUserSecuritySettings _userSecuritySettings;

        public RegistrationManager(
            IEmailService emailService, 
            IUserService userService,
            IJwtInternalToken jwtInternalService,
            IUserSecuritySettings userSecuritySettings)
        {
            _emailService = emailService;
            _userService = userService;
            _jwtInternalService = jwtInternalService;
            _userSecuritySettings = userSecuritySettings;
        }

        public async Task RegisterUserAsync (RegisterDto dto) 
        {
            var user = await _userService.CreateUserAsync(dto);


            var verificationToken = _jwtInternalService.GenerateJwtInternalToken(
              TokenPurposeSettings.EmailConfirmation,
              user.Id,
              _userSecuritySettings.EmailVerificationTokenExpirationHours);

            var link = _emailService.CreateConfirmationLink(verificationToken, PathNames.EmailConfirmation);
            string subject = "Email Confirmation";
            string body = $"Please click on the <a href ={link}>confirmation link</a> to activate your account";
            await _emailService.SendConfirmationEmail(user.Email, subject, body);

           
        }
        public async Task ResendVerificationLinkAsync(string emailAddress) 
        {
            User user;
            try
            {
                 user = await _userService.GetUserByEmailAddressAsync(emailAddress);
            }
            catch(Exception ex) when (ex is ValidationError || ex is UserException) 
            {
                return;
            }


            var verificationToken = _jwtInternalService.GenerateJwtInternalToken(
             TokenPurposeSettings.EmailConfirmation,
             user.Id,
             _userSecuritySettings.EmailVerificationTokenExpirationHours);

            var link = _emailService.CreateConfirmationLink(verificationToken, PathNames.EmailConfirmation);
            string subject = "Email Confirmation";
            string body = $"Please click on the <a href ={link}>confirmation link</a> to activate your account";
            await _emailService.SendConfirmationEmail(user.Email, subject, body);
        }

        public async Task ValidateEmailToken(string emailToken)
        {
            Guid? userId;
            try
            {
                userId = _jwtInternalService.ValidateJwtInternalToken(emailToken, TokenPurposeSettings.EmailConfirmation);



            }
            catch (SecurityTokenExpiredException)
            {
                throw new InvalidCredentialsException("Your token is expired please click on Resend Email");
            }

            if (userId is null)
                throw new NullReferenceException("Something went wrong");

            // instead return LoginResult() with token && refreshToken
            var user = await _userService.GetUserByIdAsync(userId.Value)
                    ?? throw new InvalidCredentialsException("Invalid Token");

            user.IsEmailConfirmed = true;
            user.UserStatus = UserStatusEnum.Verified;
            await _userService.UpdateUserAsync(user);
        }
    }


}
