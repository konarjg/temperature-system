using Domain.Entities;
using Domain.Entities.Util;
using Domain.Records;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.External;
using Domain.Services.Interfaces;
using Domain.Services.Util;
using Moq;

namespace UnitTests {
  public class AuthServiceTests {
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IVerificationTokenRepository> _verificationTokenRepoMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IEmailSettingsProvider> _emailSettingsProviderMock;
    private readonly AuthService _authService;

    public AuthServiceTests() {
      _tokenGeneratorMock = new Mock<ITokenGenerator>();
      _verificationTokenRepoMock = new Mock<IVerificationTokenRepository>();
      _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
      _userServiceMock = new Mock<IUserService>();
      _unitOfWorkMock = new Mock<IUnitOfWork>();
      _emailServiceMock = new Mock<IEmailService>();
      _emailSettingsProviderMock = new Mock<IEmailSettingsProvider>();

      _authService = new AuthService(
          _tokenGeneratorMock.Object,
          _verificationTokenRepoMock.Object,
          _refreshTokenRepoMock.Object,
          _userServiceMock.Object,
          _unitOfWorkMock.Object,
          _emailServiceMock.Object,
          _emailSettingsProviderMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentialsAndActiveUser_ShouldReturnAuthResult() {
      // Arrange
      User user = new() { Id = 1, Email = "test@test.com", PasswordHash = "hashed_password", Role = Role.Viewer, Deleted = null };
      RefreshToken refreshToken = new() { Token = "refresh_token", Expires = DateTime.UtcNow.AddDays(7) };
      string accessToken = "access_token";
      string email = "test@test.com";
      string password = "password";

      _userServiceMock.Setup(s => s.GetByCredentialsAsync(email, password))
          .ReturnsAsync(user);

      _tokenGeneratorMock.Setup(g => g.GenerateRefreshToken(user))
          .Returns(refreshToken);

      _tokenGeneratorMock.Setup(g => g.GenerateAccessToken(user))
          .Returns(accessToken);

      _unitOfWorkMock.Setup(uow => uow.CompleteAsync())
          .ReturnsAsync(1);

      // Act
      AuthResult? result = await _authService.LoginAsync(email, password);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(user, result.User);
      Assert.Equal(refreshToken, result.RefreshToken);
      Assert.Equal(accessToken, result.AccessToken);

      _refreshTokenRepoMock.Verify(r => r.AddAsync(refreshToken), Times.Once);
      _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldReturnNull() {
      // Arrange
      string email = "test@test.com";
      string password = "wrong_password";

      _userServiceMock.Setup(s => s.GetByCredentialsAsync(email, password))
          .ReturnsAsync((User?)null);

      // Act
      AuthResult? result = await _authService.LoginAsync(email, password);

      // Assert
      Assert.Null(result);
      _tokenGeneratorMock.Verify(g => g.GenerateRefreshToken(It.IsAny<User>()), Times.Never);
    }

    [Theory]
    [InlineData(Role.Unverified, null)]
    [InlineData(Role.Viewer, "2023-01-01")]
    public async Task LoginAsync_WithValidCredentialsButInactiveUser_ShouldReturnNull(Role role, string? deletedDate) {
      // Arrange
      DateTime? deleted = deletedDate == null ? null : DateTime.Parse(deletedDate);
      User user = new() { Id = 1, Email = "test@test.com", PasswordHash = "hashed_password", Role = role, Deleted = deleted };
      string email = "test@test.com";
      string password = "password";

      _userServiceMock.Setup(s => s.GetByCredentialsAsync(email, password))
          .ReturnsAsync(user);

      // Act
      AuthResult? result = await _authService.LoginAsync(email, password);

      // Assert
      Assert.Null(result);
      _tokenGeneratorMock.Verify(g => g.GenerateRefreshToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithNewUser_ShouldSucceedAndSendEmail() {
      // Arrange
      UserCreateData userCreateData = new("new@test.com", "password", Role.Viewer);
      User user = new() { Id = 2, Email = userCreateData.Email, PasswordHash = "hashed", Role = Role.Unverified };
      VerificationToken verificationToken = new() { Token = "verify_token", Expires = DateTime.UtcNow.AddHours(1), User = user };

      _userServiceMock.Setup(s => s.CreateAsync(userCreateData)).ReturnsAsync(user);
      _tokenGeneratorMock.Setup(g => g.GenerateVerificationToken(user)).Returns(verificationToken);
      _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);
      _emailSettingsProviderMock.Setup(p => p.VerificationUrl).Returns("http://verify.com");
      _emailServiceMock.Setup(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(), user.Email)).ReturnsAsync(true);

      // Act
      RegisterResult result = await _authService.RegisterAsync(userCreateData);

      // Assert
      Assert.Equal(RegisterState.Success, result.State);
      Assert.Equal(user, result.User);
      _verificationTokenRepoMock.Verify(r => r.AddAsync(verificationToken), Times.Once);
      _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
      _emailServiceMock.Verify(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(), user.Email), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUser_ShouldReturnConflict() {
      // Arrange
      UserCreateData userCreateData = new("existing@test.com", "password", Role.Viewer);
      _userServiceMock.Setup(s => s.CreateAsync(userCreateData)).ReturnsAsync((User?)null);

      // Act
      RegisterResult result = await _authService.RegisterAsync(userCreateData);

      // Assert
      Assert.Equal(RegisterState.Conflict, result.State);
      Assert.Null(result.User);
      _emailServiceMock.Verify(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenUnitOfWorkFails_ShouldReturnServerError() {
      // Arrange
      UserCreateData userCreateData = new("new@test.com", "password", Role.Viewer);
      User user = new() { Id = 2, Email = userCreateData.Email, PasswordHash = "hashed", Role = Role.Unverified };
      VerificationToken verificationToken = new() { Token = "verify_token", Expires = DateTime.UtcNow.AddHours(1), User = user };

      _userServiceMock.Setup(s => s.CreateAsync(userCreateData)).ReturnsAsync(user);
      _tokenGeneratorMock.Setup(g => g.GenerateVerificationToken(user)).Returns(verificationToken);
      _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(0);

      // Act
      RegisterResult result = await _authService.RegisterAsync(userCreateData);

      // Assert
      Assert.Equal(RegisterState.ServerError, result.State);
      Assert.Null(result.User);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailFails_ShouldReturnServerError() {
      // Arrange
      UserCreateData userCreateData = new("new@test.com", "password", Role.Viewer);
      User user = new() { Id = 2, Email = userCreateData.Email, PasswordHash = "hashed", Role = Role.Unverified };
      VerificationToken verificationToken = new() { Token = "verify_token", Expires = DateTime.UtcNow.AddHours(1), User = user };

      _userServiceMock.Setup(s => s.CreateAsync(userCreateData)).ReturnsAsync(user);
      _tokenGeneratorMock.Setup(g => g.GenerateVerificationToken(user)).Returns(verificationToken);
      _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);
      _emailSettingsProviderMock.Setup(p => p.VerificationUrl).Returns("http://verify.com");
      _emailServiceMock.Setup(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(), user.Email)).ReturnsAsync(false);

      // Act
      RegisterResult result = await _authService.RegisterAsync(userCreateData);

      // Assert
      Assert.Equal(RegisterState.ServerError, result.State);
      Assert.Null(result.User);
    }

    [Fact]
    public async Task RefreshAsync_WithValidToken_ShouldReturnNewAuthResult() {
      // Arrange
      User user = new() { Id = 1, Email = "test@test.com", PasswordHash = "hashed_password", Role = Role.Viewer };
      RefreshToken oldRefreshToken = new() { Token = "old_refresh_token", Expires = DateTime.UtcNow.AddDays(1), User = user };
      RefreshToken newRefreshToken = new() { Token = "new_refresh_token", Expires = DateTime.UtcNow.AddDays(7) };
      string newAccessToken = "new_access_token";

      _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync("old_refresh_token")).ReturnsAsync(oldRefreshToken);
      _tokenGeneratorMock.Setup(g => g.GenerateRefreshToken(user)).Returns(newRefreshToken);
      _tokenGeneratorMock.Setup(g => g.GenerateAccessToken(user)).Returns(newAccessToken);
      _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

      // Act
      AuthResult? result = await _authService.RefreshAsync("old_refresh_token");

      // Assert
      Assert.NotNull(result);
      Assert.Equal(newAccessToken, result.AccessToken);
      Assert.Equal(newRefreshToken, result.RefreshToken);
      Assert.NotNull(oldRefreshToken.Revoked);
      _refreshTokenRepoMock.Verify(r => r.AddAsync(newRefreshToken), Times.Once);
      _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WithInvalidOrInactiveToken_ShouldReturnNull() {
      // Arrange
      _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync("invalid_token")).ReturnsAsync((RefreshToken?)null);

      // Act
      AuthResult? result = await _authService.RefreshAsync("invalid_token");

      // Assert
      Assert.Null(result);
    }

    [Fact]
    public async Task VerifyAsync_WithValidToken_ShouldSucceed() {
      // Arrange
      User user = new() { Id = 1, Email = "test@test.com", PasswordHash = "hashed_password", Role = Role.Unverified };
      VerificationToken verificationToken = new() { Token = "valid_token", Expires = DateTime.UtcNow.AddHours(1), User = user };

      _verificationTokenRepoMock.Setup(r => r.GetByTokenAsync("valid_token")).ReturnsAsync(verificationToken);
      _userServiceMock.Setup(s => s.UpdateRoleByIdAsync(user.Id, It.Is<UserRoleUpdateData>(d => d.Role == Role.Viewer)))
          .ReturnsAsync(OperationResult.Success);
      _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

      // Act
      OperationResult result = await _authService.VerifyAsync("valid_token");

      // Assert
      Assert.Equal(OperationResult.Success, result);
      Assert.NotNull(verificationToken.Revoked);
    }

    [Fact]
    public async Task VerifyAsync_WithInvalidToken_ShouldReturnNotFound() {
      // Arrange
      _verificationTokenRepoMock.Setup(r => r.GetByTokenAsync("invalid_token")).ReturnsAsync((VerificationToken?)null);

      // Act
      OperationResult result = await _authService.VerifyAsync("invalid_token");

      // Assert
      Assert.Equal(OperationResult.NotFound, result);
    }
  }
}