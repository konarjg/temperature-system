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
    public async Task LoginAsync_ShouldReturnAuthResult_WhenLoginIsSuccessful() {
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Viewer };
        var refreshToken = new RefreshToken { Token = "refresh_token", Expires = DateTime.UtcNow.AddDays(7), User = user };
        _userServiceMock.Setup(s => s.GetByCredentialsAsync("test@test.com", "password")).ReturnsAsync(user);
        _tokenGeneratorMock.Setup(g => g.GenerateRefreshToken(user)).Returns(refreshToken);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
        _tokenGeneratorMock.Setup(g => g.GenerateAccessToken(user)).Returns("access_token");

        var result = await _authService.LoginAsync("test@test.com", "password");

        Assert.NotNull(result);
        Assert.Equal(user, result.User);
        Assert.Equal("access_token", result.AccessToken);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(refreshToken), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenCredentialsAreInvalid() {
        _userServiceMock.Setup(s => s.GetByCredentialsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((User)null);
        var result = await _authService.LoginAsync("test@test.com", "wrong_password");
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserIsInactive() {
        var inactiveUser = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Unverified };
        _userServiceMock.Setup(s => s.GetByCredentialsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(inactiveUser);
        var result = await _authService.LoginAsync("test@test.com", "password");
        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnAuthResult_WhenRefreshIsSuccessful() {
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Viewer };
        var oldRefreshToken = new RefreshToken { Token = "old_token", Expires = DateTime.UtcNow.AddDays(1), User = user };
        var newRefreshToken = new RefreshToken { Token = "new_token", Expires = DateTime.UtcNow.AddDays(7), User = user };
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("old_token")).ReturnsAsync(oldRefreshToken);
        _tokenGeneratorMock.Setup(g => g.GenerateRefreshToken(user)).Returns(newRefreshToken);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
        _tokenGeneratorMock.Setup(g => g.GenerateAccessToken(user)).Returns("new_access_token");

        var result = await _authService.RefreshAsync("old_token");

        Assert.NotNull(result);
        Assert.NotNull(oldRefreshToken.Revoked);
        Assert.Equal("new_access_token", result.AccessToken);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(newRefreshToken), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ShouldReturnTrue_WhenLogoutIsSuccessful() {
        var refreshToken = new RefreshToken { Token = "token", Expires = DateTime.UtcNow.AddDays(1), User = new User { Email = "e", PasswordHash = "p", Role = Role.Viewer } };
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("token")).ReturnsAsync(refreshToken);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var result = await _authService.LogoutAsync("token");

        Assert.True(result);
        Assert.NotNull(refreshToken.Revoked);
    }

    [Fact]
    public async Task VerifyAsync_ShouldReturnTrue_WhenVerificationIsSuccessful() {
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Unverified };
        var verificationToken = new VerificationToken { Token = "verification_token", Expires = DateTime.UtcNow.AddDays(1), User = user };
        _verificationTokenRepositoryMock.Setup(r => r.GetByTokenAsync("verification_token")).ReturnsAsync(verificationToken);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var result = await _authService.VerifyAsync("verification_token");

        Assert.True(result);
        Assert.Equal(Role.Viewer, user.Role);
        Assert.NotNull(verificationToken.Revoked);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnTrue_WhenRegistrationIsSuccessful() {
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Unverified };
        var verificationToken = new VerificationToken { Token = "verification_token", Expires = DateTime.UtcNow.AddDays(1), User = user };

        _userServiceMock.Setup(s => s.CreateAsync(user)).ReturnsAsync(user);
        _tokenGeneratorMock.Setup(g => g.GenerateVerificationToken(user)).Returns(verificationToken);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
        _emailSettingsProviderMock.Setup(p => p.GetVerificationUrl()).Returns("http://verify.com");
        _emailServiceMock.Setup(s => s.SendEmail(It.IsAny<string>(), It.IsAny<string>(), user.Email)).Returns(true);

        var result = await _authService.RegisterAsync(user);

        Assert.True(result);
        _verificationTokenRepositoryMock.Verify(r => r.AddAsync(verificationToken), Times.Once);
        _emailServiceMock.Verify(s => s.SendEmail(It.IsAny<string>(), "Thank you for choosing Temperature System, click this link to verify your account http://verify.com/verification_token", user.Email), Times.Once);
    }
}