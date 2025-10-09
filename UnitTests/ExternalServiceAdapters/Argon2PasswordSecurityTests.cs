using ExternalServiceAdapters.PasswordSecurity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTests.ExternalServiceAdapters {
  public class Argon2PasswordSecurityTests {
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Argon2PasswordSecurity _passwordSecurity;

    public Argon2PasswordSecurityTests() {
      _configurationMock = new Mock<IConfiguration>();

      // Setup mock configuration for Argon2 parameters
      _configurationMock.Setup(c => c["Security:PasswordHashing:Argon2:DegreeOfParallelism"]).Returns("8");
      _configurationMock.Setup(c => c["Security:PasswordHashing:Argon2:MemorySizeKiB"]).Returns("131072");
      _configurationMock.Setup(c => c["Security:PasswordHashing:Argon2:Iterations"]).Returns("4");
      _configurationMock.Setup(c => c["Security:PasswordHashing:Argon2:SaltSizeInBytes"]).Returns("16");
      _configurationMock.Setup(c => c["Security:PasswordHashing:Argon2:HashSizeInBytes"]).Returns("32");

      _passwordSecurity = new Argon2PasswordSecurity(_configurationMock.Object);
    }

    [Fact]
    public void HashAndVerify_WithCorrectPassword_ShouldReturnTrue() {
      // Arrange
      string password = "my-secret-password-123";

      // Act
      string hashedPassword = _passwordSecurity.Hash(password);
      bool isVerified = _passwordSecurity.Verify(password, hashedPassword);

      // Assert
      Assert.True(isVerified);
      Assert.NotNull(hashedPassword);
      Assert.NotEmpty(hashedPassword);
    }

    [Fact]
    public void HashAndVerify_WithIncorrectPassword_ShouldReturnFalse() {
      // Arrange
      string correctPassword = "my-secret-password-123";
      string incorrectPassword = "some-other-password";

      // Act
      string hashedPassword = _passwordSecurity.Hash(correctPassword);
      bool isVerified = _passwordSecurity.Verify(incorrectPassword, hashedPassword);

      // Assert
      Assert.False(isVerified);
    }
  }
}