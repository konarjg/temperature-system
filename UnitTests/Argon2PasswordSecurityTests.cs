using ExternalServiceAdapters.PasswordSecurity;
using Xunit;

namespace UnitTests;

public class Argon2PasswordSecurityTests
{
    private readonly Argon2PasswordSecurity _passwordSecurity;

    public Argon2PasswordSecurityTests()
    {
        _passwordSecurity = new Argon2PasswordSecurity();
    }

    [Fact]
    public void Hash_ShouldReturnNonEmptyString()
    {
        // Arrange
        var password = "mysecretpassword";

        // Act
        var hash = _passwordSecurity.Hash(password);

        // Assert
        Assert.False(string.IsNullOrEmpty(hash));
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void Verify_ShouldReturnTrue_ForCorrectPassword()
    {
        // Arrange
        var password = "mysecretpassword";
        var hash = _passwordSecurity.Hash(password);

        // Act
        var result = _passwordSecurity.Verify(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_ShouldReturnFalse_ForIncorrectPassword()
    {
        // Arrange
        var password = "mysecretpassword";
        var incorrectPassword = "wrongpassword";
        var hash = _passwordSecurity.Hash(password);

        // Act
        var result = _passwordSecurity.Verify(incorrectPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_ShouldReturnFalse_ForInvalidHash()
    {
        // Arrange
        var password = "mysecretpassword";
        var invalidHash = "invalidhash";

        // Act
        var result = _passwordSecurity.Verify(password, invalidHash);

        // Assert
        Assert.False(result);
    }
}