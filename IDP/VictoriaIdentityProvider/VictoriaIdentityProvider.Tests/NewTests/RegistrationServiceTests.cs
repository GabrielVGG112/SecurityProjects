using Microsoft.IdentityModel.Tokens;
using Moq;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.OptionsInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;
using VictoriaIdentityProvider.Application.Services.OrchestrationServices;
using VictoriaIdentityProvider.Domain.CustomErrors;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Tests.NewTests
{
    public class RegistrationServiceTests
    {
        RegistrationManager _registrationService;
        RegisterDto _dto;
        


        Mock<IEmailService> _emailService;
        Mock<IUserService> _userService;
        Mock<IJwtInternalToken> _jwtInternalService;
        Mock<IUserSecuritySettings> _userSecuritySettings;
        public RegistrationServiceTests()
        {
            _emailService = new Mock<IEmailService>();
            _userService = new Mock<IUserService>();
            _jwtInternalService = new Mock<IJwtInternalToken>();
            _userSecuritySettings = new Mock<IUserSecuritySettings>();
            _registrationService = new RegistrationManager(
             _emailService.Object,
             _userService.Object,
             _jwtInternalService.Object,
             _userSecuritySettings.Object);
            _dto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "0123456789",
                EmailAddress = "john@doe.com",
                Password = "StrongPassword123!",
                ConfirmPassword = "StrongPassword123!"
            };
          

        }

        [Fact]
        public async Task RegisterUserAsync_WithValidDto_ShouldSendConfirmationEmail()
        {
           
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = _dto.EmailAddress
            };

            _userService.Setup(u => u.CreateUserAsync(_dto)).ReturnsAsync(user);

            _jwtInternalService.Setup(j => j.GenerateJwtInternalToken(
                TokenPurposeSettings.EmailConfirmation,
                user.Id,
                It.IsAny<int>()))
                .Returns("token123");

            _emailService.Setup(e => e.CreateConfirmationLink("token123", PathNames.EmailConfirmation))
                .Returns("https://example.com/confirm?token=token123");

            _userSecuritySettings.SetupGet(s => s.EmailVerificationTokenExpirationHours).Returns(24);

            
            await _registrationService.RegisterUserAsync(_dto);

            _emailService.Verify(e =>
                e.SendConfirmationEmail(
                    user.Email,
                    "Email Confirmation",
                    It.Is<string>(body => body.Contains("confirmation link"))),
                Times.Once);
        }

         [Fact]
        public async Task ValidateEmailToken_WithValidToken_ShourdReturnTrue() 
        {
            Guid id = Guid.NewGuid();
            User user = new User { Id=id, Email = _dto.EmailAddress };

            _jwtInternalService.Setup(js => js.ValidateJwtInternalToken(It.IsAny<string>(), It.IsAny<string>())).Returns(id);
            _userService.Setup(us => us.GetUserByIdAsync(id)).ReturnsAsync(user);


            await  _registrationService.ValidateEmailToken("ValidEmailToken");

            _userService.Verify(us => us.UpdateUserAsync(user), Times.Once);
         
        }
        [Fact]
        public async Task ValidateToken_WithInvalidToken_SouldThrowException() 
        {
            _jwtInternalService.Setup(js => js.ValidateJwtInternalToken(It.IsAny<string>(), It.IsAny<string>())).Returns(Guid.Empty);
            _userService.Setup(us => us.GetUserByIdAsync(Guid.Empty)).ReturnsAsync((User)null!);


         await   Assert.ThrowsAsync<InvalidCredentialsException>(() => _registrationService.ValidateEmailToken("wrongToken"));
            _userService.Verify(us => us.UpdateUserAsync(It.IsAny<User>()), Times.Never);

        }


        [Fact]
        public async Task ValidateToken_WithTokenExpired_SouldThrowException()
        {
            _jwtInternalService.Setup(js => js.ValidateJwtInternalToken(It.IsAny<string>(), It.IsAny<string>())).Throws(new SecurityTokenExpiredException());

            await Assert.ThrowsAsync<InvalidCredentialsException>(() => _registrationService.ValidateEmailToken("wrongToken"));
            _userService.Verify(us => us.UpdateUserAsync(It.IsAny<User>()), Times.Never);

        }
        [Fact]
        public async Task ValidateToken_WithNullUserId_SouldThrowException()
        {
            Guid? id = null;

            _jwtInternalService.Setup(js => js.ValidateJwtInternalToken(It.IsAny<string>(), It.IsAny<string>())).Returns(id);

            await Assert.ThrowsAsync<NullReferenceException>(() => _registrationService.ValidateEmailToken("wrongToken"));
            _userService.Verify(us => us.UpdateUserAsync(It.IsAny<User>()), Times.Never);

        }

        [Fact]
        public async Task ResendVerificationLinkAsync_WithGoodEmail_ShoudReturnTrue() 
        {   
            string email = "user@email.com";

            User user = new User() {Id=Guid.NewGuid(), Email = email };
            var token = "ValidToken";

            _userService.Setup(us => us.GetUserByEmailAddressAsync(email)).ReturnsAsync(user);

            _jwtInternalService.Setup(us => us.GenerateJwtInternalToken
            (
                TokenPurposeSettings.EmailConfirmation,
                user.Id,
                It.IsAny<int>()
                ))
                .Returns(token);
            _emailService.Setup(es => es.CreateConfirmationLink(token, PathNames.EmailConfirmation)).Returns($"https://example.com/confirm?token={token}");


           await _registrationService.ResendVerificationLinkAsync(email);


            _emailService.Verify(es => es.SendConfirmationEmail(user.Email, It.IsAny<string>(), It.IsAny<string>()),Times.Once);
        }


        [Fact]
        public async Task ResendVerificationLinkAsync_WithInvalidEmailFormat_ShoudThrow()
        {
            string email = "user";

          
  

            _userService.Setup(us => us.GetUserByEmailAddressAsync(email)).ThrowsAsync(new ValidationError(new List<string>()));



          await  _registrationService.ResendVerificationLinkAsync(email);

            _emailService.Verify(es => es.CreateConfirmationLink(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _emailService.Verify(es => es.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResendVerificationLinkAsync_InvalidUser_ShoudThrow()
        {
            string email = "user@doesntExist.com";

     
       

            _userService.Setup(us => us.GetUserByEmailAddressAsync(email)).ThrowsAsync(new UserException());



            await _registrationService.ResendVerificationLinkAsync(email);

            _emailService.Verify(es => es.CreateConfirmationLink(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _emailService.Verify(es => es.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
