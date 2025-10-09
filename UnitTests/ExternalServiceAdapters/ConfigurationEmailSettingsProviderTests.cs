using ExternalServiceAdapters.EmailSettingsProvider;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace UnitTests.ExternalServiceAdapters {
  public class ConfigurationEmailSettingsProviderTests {
    [Fact]
    public void Properties_ShouldReturnValuesFromConfiguration() {
      // Arrange
      Mock<IConfiguration> configurationMock = new();

      // Mock the specific configuration keys using the indexer
      configurationMock.Setup(c => c["Email:SmtpHost"]).Returns("smtp.test.com");
      configurationMock.Setup(c => c["Email:SmtpPort"]).Returns("587");
      configurationMock.Setup(c => c["Email:SenderEmail"]).Returns("sender@test.com");
      configurationMock.Setup(c => c["Email:SenderPassword"]).Returns("password123");
      configurationMock.Setup(c => c["Email:VerificationUrl"]).Returns("http://verify.url");

      ConfigurationEmailSettingsProvider settingsProvider = new(configurationMock.Object);

      // Act
      string smtpHost = settingsProvider.SmtpHost;
      int smtpPort = settingsProvider.SmtpPort;
      string senderEmail = settingsProvider.SenderEmail;
      string senderPassword = settingsProvider.SenderPassword;
      string verificationUrl = settingsProvider.VerificationUrl;

      // Assert
      Assert.Equal("smtp.test.com", smtpHost);
      Assert.Equal(587, smtpPort);
      Assert.Equal("sender@test.com", senderEmail);
      Assert.Equal("password123", senderPassword);
      Assert.Equal("http://verify.url", verificationUrl);
    }
  }
}