using System;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Util;
using Domain.Records;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.External;
using Domain.Services.Interfaces;
using Moq;
using Xunit;

namespace UnitTests;

public class AuthServiceTests {
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IVerificationTokenRepository> _verificationTokenRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IEmailSettingsProvider> _emailSettingsProviderMock;
    private readonly AuthService _authService;

    public AuthServiceTests() {
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _verificationTokenRepositoryMock = new Mock<IVerificationTokenRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _userServiceMock = new Mock<IUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailService>();
        _emailSettingsProviderMock = new Mock<IEmailSettingsProvider>();

        _authService = new AuthService(
            _tokenGeneratorMock.Object,
            _verificationTokenRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _userServiceMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object,
            _emailSettingsProviderMock.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnTrue_WhenSuccessful() {
        // Arrange
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Unverified };
        var verificationToken = new VerificationToken { Token = "verification_token", Expires = DateTime.UtcNow.AddDays(1), User = user };

        _userServiceMock.Setup(s => s.CreateAsync(user)).ReturnsAsync(user);
        _tokenGeneratorMock.Setup(g => g.GenerateVerificationToken(user)).Returns(verificationToken);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
        _emailSettingsProviderMock.Setup(p => p.VerificationUrl).Returns("http://verify.com");
        _emailServiceMock.Setup(s => s.SendEmail(It.IsAny<string>(), It.IsAny<string>(), user.Email)).ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(user);

        // Assert
        Assert.True(result);
        _verificationTokenRepositoryMock.Verify(r => r.AddAsync(verificationToken), Times.Once);
    }

    [Fact]
    public async Task VerifyAsync_ShouldReturnTrue_WhenTokenIsValid() {
        // Arrange
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Unverified };
        var token = new VerificationToken { Token = "valid-token", Expires = DateTime.UtcNow.AddDays(1), User = user };
        _verificationTokenRepositoryMock.Setup(r => r.GetByTokenAsync("valid-token")).ReturnsAsync(token);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _authService.VerifyAsync("valid-token");

        // Assert
        Assert.True(result);
        Assert.Equal(Role.Viewer, user.Role);
        Assert.NotNull(token.Revoked);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResult_WhenCredentialsAreValidAndUserIsActive() {
        // Arrange
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Viewer }; // Active user
        var refreshToken = new RefreshToken { Token = "refresh_token", Expires = DateTime.UtcNow.AddDays(7), User = user };
        _userServiceMock.Setup(s => s.GetByCredentialsAsync("test@test.com", "password")).ReturnsAsync(user);
        _tokenGeneratorMock.Setup(g => g.GenerateRefreshToken(user)).Returns(refreshToken);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
        _tokenGeneratorMock.Setup(g => g.GenerateAccessToken(user)).Returns("access_token");

        // Act
        var result = await _authService.LoginAsync("test@test.com", "password");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access_token", result.AccessToken);
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnNewAuthResult_WhenTokenIsValid() {
        // Arrange
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Viewer };
        var oldToken = new RefreshToken { Token = "old-token", Expires = DateTime.UtcNow.AddDays(1), User = user };
        var newToken = new RefreshToken { Token = "new-token", Expires = DateTime.UtcNow.AddDays(7), User = user };
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("old-token")).ReturnsAsync(oldToken);
        _tokenGeneratorMock.Setup(g => g.GenerateRefreshToken(user)).Returns(newToken);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
        _tokenGeneratorMock.Setup(g => g.GenerateAccessToken(user)).Returns("new_access_token");

        // Act
        var result = await _authService.RefreshAsync("old-token");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new_access_token", result.AccessToken);
        Assert.NotNull(oldToken.Revoked);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(newToken), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ShouldReturnTrue_WhenTokenIsValid() {
        // Arrange
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Viewer };
        var token = new RefreshToken { Token = "valid-token", Expires = DateTime.UtcNow.AddDays(1), User = user };
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("valid-token")).ReturnsAsync(token);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _authService.LogoutAsync("valid-token");

        // Assert
        Assert.True(result);
        Assert.NotNull(token.Revoked);
    }
}