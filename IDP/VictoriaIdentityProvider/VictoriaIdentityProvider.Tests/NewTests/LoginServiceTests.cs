using Moq;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces;
using VictoriaIdentityProvider.Application.Services.ClientServices;
using VictoriaIdentityProvider.Domain.CustomErrors;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Tests.NewTests;

public class LoginServiceTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly LoginService _loginService;
    private readonly User _user;

    public LoginServiceTests()
    {
        _mockUserService = new Mock<IUserService>();

        _user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "$argon2id$hashed",
            IsEmailConfirmed = true,
            FailedLoginAttempts = 0,
            LockoutEnd = null,
            LastLogInAt = null,
            UserStatus = UserStatusEnum.Verified
        };

        _loginService = new LoginService(_mockUserService.Object, null , null);
    }

    [Fact]
    public async Task LoginUserAsync_WithValidCredentials_ShouldReturnUser()
    {
        var dto = new LoginDto { Email = "test@example.com", Password = "SecurePass123!" };

        _mockUserService.Setup(s => s.GetUserByEmailAddressAsync(dto.Email)).ReturnsAsync(_user);
        _mockUserService.Setup(s => s.ValidateEmailConfirmation(_user));
        _mockUserService.Setup(s => s.ValidateLockout(_user));
        _mockUserService.Setup(s => s.ValidatePassword(_user, dto.Password));
        _mockUserService.Setup(s => s.UpdateUserAsync(_user)).Returns(Task.CompletedTask);

        var result = await _loginService.LoginUserAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(_user.Email, result.Email);
        Assert.Equal(UserStatusEnum.Active, result.UserStatus);
        Assert.NotNull(result.LastLogInAt);
        _mockUserService.Verify(s => s.UpdateUserAsync(_user), Times.Once);
    }

    [Fact]
    public async Task LoginUserAsync_WithWrongPassword_ShouldIncrementLockoutAndThrow()
    {
        var dto = new LoginDto { Email = "test@example.com", Password = "WrongPass123!" };

        _mockUserService.Setup(s => s.GetUserByEmailAddressAsync(dto.Email)).ReturnsAsync(_user);
        _mockUserService.Setup(s => s.ValidateEmailConfirmation(_user));
        _mockUserService.Setup(s => s.ValidateLockout(_user));
        _mockUserService.Setup(s => s.ValidatePassword(_user, dto.Password))
            .Throws(new ValidationError(["Invalid password"]));
        _mockUserService.Setup(s => s.MakeUserLockout(_user)).Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<ValidationError>(() => _loginService.LoginUserAsync(dto));

        _mockUserService.Verify(s => s.MakeUserLockout(_user), Times.Once);
        _mockUserService.Verify(s => s.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginUserAsync_WithLockedAccount_ShouldThrowLockoutException()
    {
        var dto = new LoginDto { Email = "test@example.com", Password = "SecurePass123!" };

        _mockUserService.Setup(s => s.GetUserByEmailAddressAsync(dto.Email)).ReturnsAsync(_user);
        _mockUserService.Setup(s => s.ValidateEmailConfirmation(_user));
        _mockUserService.Setup(s => s.ValidateLockout(_user))
            .Throws(new LockoutExpirationException("Account locked"));

        await Assert.ThrowsAsync<LockoutExpirationException>(() => _loginService.LoginUserAsync(dto));

        _mockUserService.Verify(s => s.ValidatePassword(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginUserAsync_WithUnconfirmedEmail_ShouldThrowEmailException()
    {
        var dto = new LoginDto { Email = "test@example.com", Password = "SecurePass123!" };

        _mockUserService.Setup(s => s.GetUserByEmailAddressAsync(dto.Email)).ReturnsAsync(_user);
        _mockUserService.Setup(s => s.ValidateEmailConfirmation(_user))
            .Throws(new EmailNotVerifiedException("Email not confirmed"));

        await Assert.ThrowsAsync<EmailNotVerifiedException>(() => _loginService.LoginUserAsync(dto));

        _mockUserService.Verify(s => s.ValidateLockout(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginUserAsync_WithNonExistentEmail_ShouldThrowUserException()
    {
        var dto = new LoginDto { Email = "nobody@example.com", Password = "SecurePass123!" };

        _mockUserService.Setup(s => s.GetUserByEmailAddressAsync(dto.Email))
            .ThrowsAsync(new UserException("User not found"));

        await Assert.ThrowsAsync<UserException>(() => _loginService.LoginUserAsync(dto));

        _mockUserService.Verify(s => s.ValidateEmailConfirmation(It.IsAny<User>()), Times.Never);
    }
}
