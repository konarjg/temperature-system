using System;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Util;
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
    private readonly Mock<IPasswordSecurity> _passwordSecurityMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IEmailSettingsProvider> _emailSettingsProviderMock;
    private readonly AuthService _authService;

    public AuthServiceTests() {
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _verificationTokenRepositoryMock = new Mock<IVerificationTokenRepository>();
        _passwordSecurityMock = new Mock<IPasswordSecurity>();
        _userServiceMock = new Mock<IUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailService>();
        _emailSettingsProviderMock = new Mock<IEmailSettingsProvider>();

        _authService = new AuthService(
            _tokenGeneratorMock.Object,
            _verificationTokenRepositoryMock.Object,
            _passwordSecurityMock.Object,
            _userServiceMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object,
            _emailSettingsProviderMock.Object
        );
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

    [Fact]
    public async Task RegisterAsync_ShouldReturnFalse_WhenUserCreationFails() {
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Unverified };
        _userServiceMock.Setup(s => s.CreateAsync(user)).ReturnsAsync((User)null);

        var result = await _authService.RegisterAsync(user);

        Assert.False(result);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFalse_WhenUnitOfWorkFails() {
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Unverified };
        var verificationToken = new VerificationToken { Token = "verification_token", Expires = DateTime.UtcNow.AddDays(1), User = user };

        _userServiceMock.Setup(s => s.CreateAsync(user)).ReturnsAsync(user);
        _tokenGeneratorMock.Setup(g => g.GenerateVerificationToken(user)).Returns(verificationToken);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

        var result = await _authService.RegisterAsync(user);

        Assert.False(result);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFalse_WhenEmailSendingFails() {
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Unverified };
        var verificationToken = new VerificationToken { Token = "verification_token", Expires = DateTime.UtcNow.AddDays(1), User = user };

        _userServiceMock.Setup(s => s.CreateAsync(user)).ReturnsAsync(user);
        _tokenGeneratorMock.Setup(g => g.GenerateVerificationToken(user)).Returns(verificationToken);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
        _emailSettingsProviderMock.Setup(p => p.GetVerificationUrl()).Returns("http://verify.com");
        _emailServiceMock.Setup(s => s.SendEmail(It.IsAny<string>(), It.IsAny<string>(), user.Email)).Returns(false);

        var result = await _authService.RegisterAsync(user);

        Assert.False(result);
    }
}