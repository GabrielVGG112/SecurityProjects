using Microsoft.Extensions.Options;
using Moq;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Enums;
using VictoriaIdentityProvider.Application.Interfaces.OptionsInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;
using VictoriaIdentityProvider.Application.Services.ClientServices;
using VictoriaIdentityProvider.Application.Services.OrchestrationServices;
using VictoriaIdentityProvider.Application.Validators;
using VictoriaIdentityProvider.Application.Validators.InputValidation;
using VictoriaIdentityProvider.Domain.CustomErrors;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Tests.NewTests
{
    public class UserServiceTests
    {

        private readonly User _user;

        Mock<IUserRepository> _userRepo;
        Mock<IHasher> _passwordHasher;
        FieldValidatorFacade _validatorFacade;
        Mock<IUserSecuritySettings> _userSecuritySettings;
        UserService _userService;
        IOptions<SecurityOptions> _options;
        Mock<IRefreshTokenRepository> _refreshTokenRepository;
        public UserServiceTests()
        {
            _userRepo = new Mock<IUserRepository>();
            _passwordHasher = new Mock<IHasher>();
            _userSecuritySettings = new Mock<IUserSecuritySettings>();
            _passwordHasher.Setup(p => p.HashData(It.IsAny<string>())).Returns("$argon2id$hashed");
            _passwordHasher.Setup(p => p.VerifyData(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var securityOptions = new SecurityOptions
            {
                PasswordRequireDigit = true,
                PasswordRequireLowercase = true,
                PasswordRequireUppercase = true,
                CheckBreachedPasswords = false,
                PasswordMinLength = 8,
                PasswordMaxLength = 128,
                MaxLoginAttempts = 3,
                RequireEmailVerification = false,
                LockoutDurationMinutes = 15,
                EmailVerificationTokenExpirationHours = 24,
                PasswordResetTokenExpirationHours = 24,
            };
            _options = Options.Create(securityOptions);
            _refreshTokenRepository = new Mock<IRefreshTokenRepository>();
            _validatorFacade =
                new FieldValidatorFacade(
                    emailValidator: new EmailValidator(_options),
                    passwordValidator: new PasswordValidator(_options,null),
                    nameValidator: new NameValidator(),
                    phoneValidator: new PhoneNumberValidator()
                   );


            _user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "Test",
                Email = "test@example.com",
                IsEmailConfirmed = true,
                Password = "$argon2id$hashed",
                LockoutEnd = null,
                LastLogInAt = null,
                UserStatus = UserStatusEnum.Verified,
            };

            _userService = new UserService(
                 _validatorFacade,
                 _userRepo.Object,
                 _userSecuritySettings.Object,
                 _passwordHasher.Object
                 );
        }
        [Fact]

        public async Task GetByEmailAsync_WithValidEmail_ShouldReturnTrue()
        {
            string email = "test@example.com";
            _userRepo.Setup(r => r.GetUserByEmailAsync(email)).ReturnsAsync(_user);

            await _userService.GetUserByEmailAddressAsync(email);
            _userRepo.Verify(r => r.GetUserByEmailAsync(email), Times.Once());

            Assert.Equal(email, _user.Email);


        }
        [Fact]
        public async Task GetByEmailAsync_WithInvalidUser_ShoudReturnFalse()
        {
            string email = "george@example.com";

            _userRepo.Setup(r => r.GetUserByEmailAsync(email)).ReturnsAsync((User)null!);



            await Assert.ThrowsAsync<UserException>(() => _userService.GetUserByEmailAddressAsync(email));
            _userRepo.Verify(r => r.GetUserByEmailAsync(email), Times.Once());
        }

        [Fact]
        public async Task GetByEmailAsync_WithInvalidUser_ShouldThrowException()
        {
            string email = "InvalidEmail";

            var ex = await Assert.ThrowsAsync<ValidationError>(() => _userService.GetUserByEmailAddressAsync(email));
            Assert.Contains("Invalid email format", string.Join(',', ex.Errors));
        }

        [Fact]
        public async Task GetByEmailAsync_WithControlChar_ShoudThrowExceptionWithRightMessage()
        {
            string email = "invalidEmail@gmail\x07.com";

            var ex = await Assert.ThrowsAsync<ValidationError>(() => _userService.GetUserByEmailAddressAsync(email));

            Assert.Contains("Email contains invalid characters", string.Join(",", ex.Errors));

        }
        // create user
        [Fact]
        public async Task CreateUserAsync_WithValidRegisterDto_ShouldReturnTrue()
        {
            RegisterDto dto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "0123456789",
                EmailAddress = "john@doe.com",
                Password = "StrongPassword123!",
                ConfirmPassword = "StrongPassword123!"
            };

            _userRepo.Setup(r => r.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);



            var user = await _userService.CreateUserAsync(dto);

            _userRepo.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Once);
            _passwordHasher.Verify(r => r.HashData(It.IsAny<string>()), Times.Once);

            Assert.Equal(dto.FirstName, user.FirstName);
            Assert.Equal(dto.LastName, user.LastName);
            Assert.Equal(dto.EmailAddress, user.Email);
            Assert.Equal("$argon2id$hashed", user.Password);
            Assert.Equal(UserStatusEnum.PendingVerification, user.UserStatus);
            Assert.False(user.IsEmailConfirmed);
            Assert.Equal(0, user.FailedLoginAttempts);
            Assert.Null(user.LockoutEnd);
            Assert.False(user.IsLocked);
        }


        [Fact]

        public async Task CreateUserAsync_WithInvalidPassword_ShouldThrowSpecificException()
        {
            RegisterDto dto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "0123456789",
                EmailAddress = "john@doe.com",
                Password = "StrongPassword123!",
                ConfirmPassword = "StrongPassword13!"
            };


            var ex = await Assert.ThrowsAsync<ValidationError>(() => _userService.CreateUserAsync(dto));

            Assert.Contains("Passwords do not match", string.Join(" ", ex.Errors));
        }
        [Fact]
        public async Task CreateUserAsync_WithMultipleInvalidFields_ShoudReturnRightErrorNames()
        {
            RegisterDto dto = new RegisterDto
            {
                FirstName = "J",
                LastName = "D",
                PhoneNumber = "01234589",
                EmailAddress = "john@doe.com", // correct
                Password = "Strong",
                ConfirmPassword = "Strong"
            };

            _userRepo.Setup(r => r.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAsync<ValidationError>(() => _userService.CreateUserAsync(dto));

            _userRepo.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
            Assert.Contains(ValidationField.FirstName.ToString(), string.Join(",", ex.Errors));
            Assert.Contains(ValidationField.LastName.ToString(), string.Join(",", ex.Errors));
            Assert.Contains(ValidationField.PhoneNumber.ToString(), string.Join(",", ex.Errors));
            Assert.Contains(ValidationField.Password.ToString(), string.Join(",", ex.Errors));


        }
        [Fact]
        public async Task CreateUserAsync_WithTwoInvalidFields_ShoudReturnRightErrorNames()
        {
            RegisterDto dto = new RegisterDto
            {
                FirstName = "John",
                LastName = "D", // false
                PhoneNumber = "0123456789",
                EmailAddress = "johndoe.com", // false
                Password = "StrongPassword123!",
                ConfirmPassword = "StrongPassword123!"
            };

            _userRepo.Setup(r => r.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAsync<ValidationError>(() => _userService.CreateUserAsync(dto));

            _userRepo.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
            Assert.Contains(ValidationField.Email.ToString(), string.Join(",", ex.Errors));
            Assert.Contains(ValidationField.LastName.ToString(), string.Join(",", ex.Errors));


        }
        [Fact]
        public async Task CreateUserAsync_WithControlCharFields_ShoudReturnRightErrorNamesAndMessages()
        {
            RegisterDto dto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe", // false
                PhoneNumber = "0123456789",
                EmailAddress = "john@doe\n.com", // false
                Password = "StrongPassword\n123!",
                ConfirmPassword = "StrongPassword\n123!"
            };

            _userRepo.Setup(r => r.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAsync<ValidationError>(() => _userService.CreateUserAsync(dto));

            _userRepo.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
            Assert.Contains(string.Format("{0} : {1}", ValidationField.Email.ToString(), "Email contains invalid characters"), string.Join(",", ex.Errors));
            Assert.Contains(string.Format("{0} : {1}", ValidationField.Password.ToString(), "Password contains invalid characters"), string.Join(",", ex.Errors));

        }


        [Fact]
        public async Task CreateUserAsync_WhenEmailAlreadyExists_ShouldThrowUserException()

        {
            RegisterDto dto = new RegisterDto
            {
                FirstName = _user.FirstName,
                LastName = _user.LastName, // false
                PhoneNumber = "0123456789",
                EmailAddress = _user.Email, // false
                Password = "StrongPassword123!",
                ConfirmPassword = "StrongPassword123!"
            };
            _userRepo.Setup(r => r.VerifyIfEmailExistsAsync(_user.Email)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<UserException>(() => _userService.CreateUserAsync(dto));
            _userRepo.Verify(r => r.VerifyIfEmailExistsAsync(_user.Email), Times.Once);
            Assert.Contains("An user with same e-mail address exists  allredy", ex.Message);
        }
        [Fact]
        public async Task GetUserByIdAsync_WithEmptyGuid_ShouldThrowException()
        {
            Guid id = Guid.Empty;

            var ex = await Assert.ThrowsAsync<UserException>(() => _userService.GetUserByIdAsync(id));

            Assert.Contains("mandatory", ex.Message);
        }
        [Fact]
        public async Task GetUserById_WithNonExistentUserId_ShouldThrowException() 
        {
            Guid id = Guid.NewGuid();
    #pragma warning disable CS8620,CS8600    
            _userRepo.Setup(r => r.GetUserByIdAsync(id)).ReturnsAsync((User)null);

            var ex = await Assert.ThrowsAsync<UserException>(() => _userService.GetUserByIdAsync(id));

            _userRepo.Verify(r => r.GetUserByIdAsync(id), Times.Once);
            Assert.Contains("doesnt exist", ex.Message);
        }
        [Fact]
        public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnUser()
        {
            Guid id = Guid.NewGuid();

            _userRepo.Setup(r => r.GetUserByIdAsync(id)).ReturnsAsync(_user);

            var user = await _userService.GetUserByIdAsync(id);

            _userRepo.Verify(r => r.GetUserByIdAsync(id), Times.Once);
            Assert.Equal(_user.Email, user.Email);
        }
        [Fact]
        public async Task ValidatePassword_WithInvalidPasswordFormat_ShouldThrowValidationError() 
        {
            string password = "short";

            var ex = await Assert.ThrowsAsync<ValidationError>(() =>  _userService.ValidatePassword(_user, password));

            Assert.Contains("Password", string.Join(',', ex.Errors));
        }
        [Fact]
        public async Task ValidatePassword_WithWrongPassword_ShouldThrowValidationError() 
        {
            string password = _user.Password + "abcde";
            _passwordHasher.Setup(h => h.VerifyData(password, _user.Password)).Returns(false);

            var ex = Assert.ThrowsAsync<ValidationError>(() => _userService.ValidatePassword(_user, password));
        }
        [Fact]
        public void ValidateLockout_WithLockoutEnabled_ShouldThrowException() 
        {
            _user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
            Assert.Throws<LockoutExpirationException>(() =>_userService.ValidateLockout(_user));

        }
        [Fact]
        public void ValidateLockout_WithLockoutDisabled_ShoudReturnTrue() 
        {
            _user.LockoutEnd = null;
            _userService.ValidateLockout(_user);
        }
        [Fact]
        public async Task UpdateUserAsync_WithNonExistingUser_ShouldThrowException()
        {
            var user = new User { Id = Guid.NewGuid() };

            _userRepo.Setup(r => r.VerifyIfIdExistsAsync(user.Id)).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<UserException>(() => _userService.UpdateUserAsync(user));

            _userRepo.Verify(r => r.VerifyIfIdExistsAsync(user.Id), Times.Once);
            _userRepo.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
            Assert.Contains("does not exist", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        [Fact]
        public async Task UpdateUserAsync_WithExistingUser_ShouldUpdateSuccessfully()
        {
            var user = new User { Id = Guid.NewGuid() };

            _userRepo.Setup(r => r.VerifyIfIdExistsAsync(user.Id)).ReturnsAsync(true);

            await _userService.UpdateUserAsync(user);

            _userRepo.Verify(r => r.VerifyIfIdExistsAsync(user.Id), Times.Once);
            _userRepo.Verify(r => r.UpdateUserAsync(user), Times.Once);
        }
    } 
}