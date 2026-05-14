using VictoriaIdentityProvider.Application.Services.Factory;

namespace VictoriaIdentityProvider.Tests.NewTests.Validators;

public class ValidationResultTests
{
    [Fact]
    public void Success_ShouldCreateValidResult()
    {
       
        var result = ValidationResult.Success();

      
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_WithSingleError_ShouldCreateInvalidResult()
    {
       
        var errorMessage = "Validation failed";

       
        var result = ValidationResult.Failure(errorMessage);

      
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains(errorMessage, result.Errors);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ShouldCreateInvalidResult()
    {
       
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

       
        var result = ValidationResult.Failure(errors);

      
        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Count);
        Assert.Contains("Error 1", result.Errors);
        Assert.Contains("Error 2", result.Errors);
        Assert.Contains("Error 3", result.Errors);
    }

    [Fact]
    public void Failure_WithEmptyErrorList_ShouldCreateInvalidResult()
    {
       
        var errors = new List<string>();

        
        var result = ValidationResult.Failure(errors);

       
        Assert.False(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Success_ShouldBeImmutable()
    {
        
        var result = ValidationResult.Success();

        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Failure_ShouldBeImmutable()
    {
        
        var result = ValidationResult.Failure("Error");

       
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void MultipleSuccessCalls_ShouldCreateIndependentInstances()
    {
        // Act
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        // Assert
        Assert.NotSame(result1, result2);
        Assert.True(result1.IsValid);
        Assert.True(result2.IsValid);
    }

    [Fact]
    public void MultipleFailureCalls_WithSameError_ShouldCreateIndependentInstances()
    {
        var error = "Same error";

       
        var result1 = ValidationResult.Failure(error);
        var result2 = ValidationResult.Failure(error);

        Assert.NotSame(result1, result2);
        Assert.False(result1.IsValid);
        Assert.False(result2.IsValid);
    }
}
