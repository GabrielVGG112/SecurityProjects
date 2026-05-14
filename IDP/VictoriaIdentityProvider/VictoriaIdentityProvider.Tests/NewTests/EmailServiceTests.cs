using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.Services;
using VictoriaIdentityProvider.Application.Services.ClientServices;
using Xunit;

namespace VictoriaIdentityProvider.Tests.NewTests;

public class EmailServiceTests
{
    [Fact]
    public void Constructor_WithOptions_SetsOptionsValue()
    {
        // Arrange
        var smtpOptions = new SmtpOptions
        {
            Host = "smtp.test.com",
            Port = 587
        };
        var options = Options.Create(smtpOptions);

        // Act
        var service = new EmailService(options);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithOptionsAndLogger_SetsOptionsValue()
    {
        // Arrange
        var smtpOptions = new SmtpOptions
        {
            Host = "smtp.test.com",
            Port = 587
        };
        var options = Options.Create(smtpOptions);
     

        // Act
        var service = new EmailService(options);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithOptionsAndNullLogger_SetsOptionsValue()
    {
        // Arrange
        var smtpOptions = new SmtpOptions
        {
            Host = "smtp.test.com",
            Port = 587
        };
        var options = Options.Create(smtpOptions);

        // Act
        var service = new EmailService(options);

        // Assert
        Assert.NotNull(service);
    }
}
