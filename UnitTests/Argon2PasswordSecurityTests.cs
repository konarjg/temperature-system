using ExternalServiceAdapters.PasswordSecurity;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace UnitTests;

public class Argon2PasswordSecurityTests
{
    private readonly Argon2PasswordSecurity _passwordSecurity;

    public Argon2PasswordSecurityTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("Security:PasswordHashing:Argon2:DegreeOfParallelism", "1"),
                new KeyValuePair<string, string>("Security:PasswordHashing:Argon2:MemorySizeKiB", "8192"),
                new KeyValuePair<string, string>("Security:PasswordHashing:Argon2:Iterations", "1"),
                new KeyValuePair<string, string>("Security:PasswordHashing:Argon2:SaltSizeInBytes", "16"),
                new KeyValuePair<string, string>("Security:PasswordHashing:Argon2:HashSizeInBytes", "32"),
            }!)
            .Build();
        _passwordSecurity = new Argon2PasswordSecurity(configuration);
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