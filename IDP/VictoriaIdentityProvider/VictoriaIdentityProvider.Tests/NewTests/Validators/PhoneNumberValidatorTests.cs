using Microsoft.Extensions.Logging;
using Moq;
using VictoriaIdentityProvider.Application.Validators.InputValidation;

namespace VictoriaIdentityProvider.Tests.NewTests.Validators;

public class PhoneNumberValidatorTests
{
    private readonly PhoneNumberValidator _validator;
    private readonly Mock<ILogger<PhoneNumberValidator>> _mockLogger;

    public PhoneNumberValidatorTests()
    {
        _mockLogger = new Mock<ILogger<PhoneNumberValidator>>();
        _validator = new PhoneNumberValidator(_mockLogger.Object);
    }

    [Fact]
    public void Validate_WithNullPhoneNumber_ShouldReturnFailure()
    {
        // Arrange
        string phoneNumber = null!;

        // Act
        var result = _validator.Validate(phoneNumber);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Phone number is required", result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyPhoneNumber_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumber = "";

        // Act
        var result = _validator.Validate(phoneNumber);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Phone number is required", result.Errors);
    }

    [Fact]
    public void Validate_WithWhitespacePhoneNumber_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumber = "   ";

        // Act
        var result = _validator.Validate(phoneNumber);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Phone number is required", result.Errors);
    }

    [Fact]
    public void Validate_WithPhoneNumberTooLong_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumber = "123456789012345678901"; // 21 characters

        // Act
        var result = _validator.Validate(phoneNumber);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Phone number is too long", result.Errors);
    }

    [Fact]
    public void Validate_WithControlCharacters_ShouldReturnFailureAndLogCritical()
    {
        // Arrange
        var phoneNumber = "1234567\n890"; // Contains newline control character in the middle

        // Act
        var result = _validator.Validate(phoneNumber);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Phone number contains invalid control characters", result.Errors);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attemt to use Control Characters")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Validate_WithLetters_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumber = "123456789a";

        // Act
        var result = _validator.Validate(phoneNumber);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Phone number can't contain letters", result.Errors);
    }

    [Fact]
    public void Validate_WithInvalidPhoneFormat_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumber = "##########"; // 10 characters but invalid format for PhoneAttribute

        // Act
        var result = _validator.Validate(phoneNumber);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid phone number format", result.Errors);
    }

    [Fact]
    public void Validate_WithValidPhoneNumber_ShouldReturnSuccess()
    {
        // Arrange
        var phoneNumber = "+1234567890";

        // Act
        var result = _validator.Validate(phoneNumber);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var phoneNumber = "123"; // Too short

        // Act
        var result = _validator.Validate(phoneNumber);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Phone number is too short", result.Errors);
    }

    [Theory]
    [InlineData("+12345678901")]
    [InlineData("1234567890")]
    [InlineData("+1-234-567-8901")]
    public void Validate_WithVariousValidFormats_ShouldReturnSuccess(string phoneNumber)
    {
        // Act
        var result = _validator.Validate(phoneNumber);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithPhoneNumberContainingSpaces_ShouldTrimAndValidate()
    {
        // Arrange
        var phoneNumber = "  +1234567890  ";

        // Act
        var result = _validator.Validate(phoneNumber);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithoutLogger_ShouldStillValidate()
    {
        // Arrange
        var validatorWithoutLogger = new PhoneNumberValidator();
        var phoneNumber = "+1234567890";

        // Act
        var result = validatorWithoutLogger.Validate(phoneNumber);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithControlCharactersAndNoLogger_ShouldReturnFailure()
    {
        // Arrange
        var validatorWithoutLogger = new PhoneNumberValidator();
        var phoneNumber = "12345\t67890"; // Tab control character in the middle

        // Act
        var result = validatorWithoutLogger.Validate(phoneNumber);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Phone number contains invalid control characters", result.Errors);
    }
}
