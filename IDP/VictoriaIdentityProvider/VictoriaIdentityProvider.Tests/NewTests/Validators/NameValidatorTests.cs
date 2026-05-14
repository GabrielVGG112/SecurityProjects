using Microsoft.Extensions.Logging;
using Moq;
using VictoriaIdentityProvider.Application.Validators.InputValidation;

namespace VictoriaIdentityProvider.Tests.NewTests.Validators;

public class NameValidatorTests
{
    [Fact]
    public void Validate_WithDigitsInName_ShouldLogCritical()
    {
       
        var mockLogger = new Mock<ILogger<NameValidator>>();
        var validator = new NameValidator();
        var nameWithDigit = "John123";

       
        var result = validator.Validate(nameWithDigit);

      
        Assert.False(result.IsValid);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attemt to use Control Characters")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void Validate_WithDigitsInName_ShouldLogCriticalWithCorrectParameterName()
    {
       
        var mockLogger = new Mock<ILogger<NameValidator>>();
        var validator = new NameValidator();
        var nameWithDigit = "Alice3";

       
        var result = validator.Validate(nameWithDigit);

       
        Assert.False(result.IsValid);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("name")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void Validate_WithMultipleDigitsInName_ShouldLogCriticalOnce()
    {
    
        var mockLogger = new Mock<ILogger<NameValidator>>();
        var validator = new NameValidator();
        var nameWithMultipleDigits = "Bob12345";

       
        var result = validator.Validate(nameWithMultipleDigits);

      
        Assert.False(result.IsValid);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attemt to use Control Characters")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void Validate_WithNoLogger_ShouldNotThrowException()
    {
      
        var validator = new NameValidator();
        var nameWithDigit = "John123";

     
        var result = validator.Validate(nameWithDigit);

        
        Assert.False(result.IsValid);
        Assert.Contains("Name can't contain digits", result.Errors);
    }

    [Fact]
    public void Validate_WithoutDigitsInName_ShouldNotLogCritical()
    {
        
        var mockLogger = new Mock<ILogger<NameValidator>>();
        var validator = new NameValidator();
        var validName = "John";

     
        var result = validator.Validate(validName);

       
        Assert.True(result.IsValid);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void Validate_WithDigitsInName_ShouldAddDigitsError()
    {
      
        var mockLogger = new Mock<ILogger<NameValidator>>();
        var validator = new NameValidator();
        var nameWithDigit = "Test1";

    
        var result = validator.Validate(nameWithDigit);

      
        Assert.False(result.IsValid);
        Assert.Contains("Name can't contain digits", result.Errors);
    }

    [Fact]
    public void Validate_WithDigitAtStart_ShouldLogCritical()
    {
    
        var mockLogger = new Mock<ILogger<NameValidator>>();
        var validator = new NameValidator();
        var nameWithDigit = "1John";

   
        var result = validator.Validate(nameWithDigit);

        Assert.False(result.IsValid);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attemt to use Control Characters")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
        Assert.Contains("Name can't contain digits", result.Errors);
    }

    [Fact]
    public void Validate_WithDigitAtEnd_ShouldLogCritical()
    {
   
        var mockLogger = new Mock<ILogger<NameValidator>>();
        var validator = new NameValidator();
        var nameWithDigit = "John9";

       
        var result = validator.Validate(nameWithDigit);

  
        Assert.False(result.IsValid);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attemt to use Control Characters")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
        Assert.Contains("Name can't contain digits", result.Errors);
    }
}
