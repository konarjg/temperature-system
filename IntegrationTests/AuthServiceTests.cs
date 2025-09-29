
using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Util;
using Domain.Services.External;
using Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace IntegrationTests;

public class AuthServiceTests : BaseServiceTests {
    private readonly IAuthService _authService;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IPasswordSecurity> _passwordSecurityMock;
    private readonly Mock<IEmailSettingsProvider> _emailSettingsProviderMock;

    public AuthServiceTests() {
        _authService = ServiceProvider.GetRequiredService<IAuthService>();
        _tokenGeneratorMock = Mock.Get(ServiceProvider.GetRequiredService<ITokenGenerator>());
        _emailServiceMock = Mock.Get(ServiceProvider.GetRequiredService<IEmailService>());
        _passwordSecurityMock = Mock.Get(ServiceProvider.GetRequiredService<IPasswordSecurity>());
        _emailSettingsProviderMock = Mock.Get(ServiceProvider.GetRequiredService<IEmailSettingsProvider>());

        _passwordSecurityMock.Setup(p => p.Hash(It.IsAny<string>())).Returns((string s) => "hashed_" + s);
        _passwordSecurityMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string pass, string hash) => hash == "hashed_" + pass);
        _emailSettingsProviderMock.Setup(p => p.VerificationUrl).Returns("http://verify.com");
        _emailServiceMock.Setup(s => s.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUnverifiedUserAndSendVerificationEmail() {
        // Arrange
        _tokenGeneratorMock.Setup(g => g.GenerateVerificationToken(It.IsAny<User>()))
            .Returns((User u) => new VerificationToken { Token = "verify-123", Expires = DateTime.UtcNow.AddDays(1), User = u });

        // Act
        var userToRegister = new User { Email = "register@example.com", PasswordHash = "password", Role = Role.Unverified };
        var result = await _authService.RegisterAsync(userToRegister);

        // Assert
        Assert.True(result);
        var user = await DbContext.Users.SingleOrDefaultAsync(u => u.Email == "register@example.com");
        Assert.NotNull(user);
        Assert.Equal(Role.Unverified, user.Role);
        var token = await DbContext.VerificationTokens.SingleOrDefaultAsync(t => t.User.Id == user.Id);
        Assert.NotNull(token);

        // Verify email was sent with the correct link
        _emailServiceMock.Verify(s => s.SendEmail(
            "Verify your email",
            It.Is<string>(body => body.Contains("http://verify.com?token=verify-123")),
            "register@example.com"
        ), Times.Once);
    }

    [Fact]
    public async Task VerifyAsync_ShouldActivateUserAndRevokeToken() {
        // Arrange
        var user = new User { Email = "verify@example.com", PasswordHash = "hashed_password", Role = Role.Unverified };
        var token = new VerificationToken { Token = "verify-token-to-revoke", Expires = DateTime.UtcNow.AddDays(1), User = user };
        DbContext.Users.Add(user);
        DbContext.VerificationTokens.Add(token);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _authService.VerifyAsync(token.Token);

        // Assert
        Assert.True(result);
        await DbContext.Entry(user).ReloadAsync();
        Assert.Equal(Role.Viewer, user.Role);
        await DbContext.Entry(token).ReloadAsync();
        Assert.NotNull(token.Revoked);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResult_WhenCredentialsAreValid() {
        // Arrange
        var user = new User { Email = "login@example.com", PasswordHash = "hashed_password", Role = Role.Viewer };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();
        var refreshToken = new RefreshToken { Token = "refresh-1", Expires = DateTime.UtcNow.AddDays(7), User = user };
        _tokenGeneratorMock.Setup(g => g.GenerateRefreshToken(user)).Returns(refreshToken);
        _tokenGeneratorMock.Setup(g => g.GenerateAccessToken(user)).Returns("access-token-1");

        // Act
        var result = await _authService.LoginAsync("login@example.com", "password");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token-1", result.AccessToken);
        Assert.Single(DbContext.RefreshTokens.Where(rt => rt.User.Id == user.Id));
        
        // Verify mocks were called
        _passwordSecurityMock.Verify(p => p.Verify("password", "hashed_password"), Times.Once);
        _tokenGeneratorMock.Verify(g => g.GenerateAccessToken(It.Is<User>(u => u.Id == user.Id)), Times.Once);
        _tokenGeneratorMock.Verify(g => g.GenerateRefreshToken(It.Is<User>(u => u.Id == user.Id)), Times.Once);
    }
}
