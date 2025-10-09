using Domain.Entities;
using Domain.Entities.Util;
using ExternalServiceAdapters.TokenGenerator;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace UnitTests.ExternalServiceAdapters {
  public class JwtTokenGeneratorTests {
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly JwtTokenGenerator _tokenGenerator;

    public JwtTokenGeneratorTests() {
      _configurationMock = new Mock<IConfiguration>();

      // Setup mock configuration for JWT settings
      _configurationMock.Setup(c => c["Jwt:Key"]).Returns("a-super-secret-key-that-is-long-enough-for-hs256");
      _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("test-issuer");
      _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("test-audience");
      _configurationMock.Setup(c => c["Jwt:AccessTokenLifetimeMinutes"]).Returns("15");
      _configurationMock.Setup(c => c["Jwt:RefreshTokenLifetimeDays"]).Returns("7");
      _configurationMock.Setup(c => c["Jwt:UserVerificationTokenExpirationDays"]).Returns("1"); // Use the correct key

      _tokenGenerator = new JwtTokenGenerator(_configurationMock.Object);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainCorrectClaims() {
      // Arrange
      User user = new() {
        Id = 123,
        Email = "test@test.com",
        Role = Role.Admin,
        PasswordHash = "some_hash"
      };

      // Act
      string tokenString = _tokenGenerator.GenerateAccessToken(user);

      // Assert
      Assert.NotNull(tokenString);
      JwtSecurityTokenHandler handler = new();
      JwtSecurityToken decodedToken = handler.ReadJwtToken(tokenString);

      Claim? idClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
      Claim? roleClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

      Assert.NotNull(idClaim);
      Assert.Equal(user.Id.ToString(), idClaim.Value);
      Assert.NotNull(roleClaim);
      Assert.Equal(user.Role.ToString(), roleClaim.Value);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldHaveCorrectExpiration() {
      // Arrange
      User user = new() { Id = 1, Email = "test@test.com", PasswordHash = "hash", Role = Role.Viewer };
      System.DateTime expectedExpiration = System.DateTime.UtcNow.AddDays(7);

      // Act
      RefreshToken refreshToken = _tokenGenerator.GenerateRefreshToken(user);

      // Assert
      Assert.NotNull(refreshToken);
      Assert.NotEmpty(refreshToken.Token);
      Assert.InRange(refreshToken.Expires, expectedExpiration.AddSeconds(-5), expectedExpiration.AddSeconds(5));
    }

    [Fact]
    public void GenerateVerificationToken_ShouldHaveCorrectExpiration() {
      // Arrange
      User user = new() { Id = 1, Email = "test@test.com", PasswordHash = "hash", Role = Role.Unverified };
      System.DateTime expectedExpiration = System.DateTime.UtcNow.AddDays(1); // Expect days, not hours

      // Act
      VerificationToken verificationToken = _tokenGenerator.GenerateVerificationToken(user);

      // Assert
      Assert.NotNull(verificationToken);
      Assert.NotEmpty(verificationToken.Token);
      Assert.InRange(verificationToken.Expires, expectedExpiration.AddSeconds(-5), expectedExpiration.AddSeconds(5));
    }
  }
}