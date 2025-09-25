using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Domain.Entities;
using Domain.Entities.Util;
using ExternalServiceAdapters.TokenGenerator;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace UnitTests;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _tokenGenerator;
    private readonly User _testUser;

    public JwtTokenGeneratorTests()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("Jwt:Key", "a-super-secret-key-that-is-long-enough"),
                new KeyValuePair<string, string>("Jwt:Issuer", "TestIssuer"),
                new KeyValuePair<string, string>("Jwt:Audience", "TestAudience"),
                new KeyValuePair<string, string>("Jwt:AccessTokenExpirationMinutes", "15"),
                new KeyValuePair<string, string>("Jwt:RefreshTokenExpirationDays", "7"),
                new KeyValuePair<string, string>("Jwt:VerificationTokenExpirationDays", "1")
            }!)
            .Build();

        _tokenGenerator = new JwtTokenGenerator(configuration);
        _testUser = new User { Id = 1, Email = "test@example.com", Role = Role.Viewer, PasswordHash = "some_hash" };
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainCorrectClaims()
    {
        // Act
        string tokenString = _tokenGenerator.GenerateAccessToken(_testUser);
        JwtSecurityTokenHandler tokenHandler = new();
        JwtSecurityToken token = tokenHandler.ReadJwtToken(tokenString);

        // Assert
        Assert.NotNull(token);
        Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        Claim? userEmailClaim = token.Claims.FirstOrDefault(c => c.Type == "email");
        Claim? userRoleClaim = token.Claims.FirstOrDefault(c => c.Type == "role");

        Assert.Equal(_testUser.Id.ToString(), userIdClaim?.Value);
        Assert.Equal(_testUser.Email, userEmailClaim?.Value);
        Assert.Equal(_testUser.Role.ToString(), userRoleClaim?.Value);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldBeAssociatedWithUser()
    {
        // Act
        RefreshToken refreshToken = _tokenGenerator.GenerateRefreshToken(_testUser);

        // Assert
        Assert.NotNull(refreshToken);
        Assert.False(string.IsNullOrEmpty(refreshToken.Token));
        Assert.Equal(_testUser, refreshToken.User);
        Assert.True(refreshToken.Expires > DateTime.UtcNow);
    }

    [Fact]
    public void GenerateVerificationToken_ShouldBeAssociatedWithUser()
    {
        // Act
        VerificationToken verificationToken = _tokenGenerator.GenerateVerificationToken(_testUser);

        // Assert
        Assert.NotNull(verificationToken);
        Assert.False(string.IsNullOrEmpty(verificationToken.Token));
        Assert.Equal(_testUser, verificationToken.User);
        Assert.True(verificationToken.Expires > DateTime.UtcNow);
    }
}