using Domain.Entities;
using Domain.Entities.Util;
using Domain.Records;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.External;
using Domain.Services.Util;
using Moq;
using System.Threading.Tasks;

namespace UnitTests {
  public class UserServiceTests {
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordSecurity> _passwordSecurityMock;
    private readonly UserService _userService;

    public UserServiceTests() {
      _userRepositoryMock = new Mock<IUserRepository>();
      _unitOfWorkMock = new Mock<IUnitOfWork>();
      _passwordSecurityMock = new Mock<IPasswordSecurity>();

      _userService = new UserService(
          _userRepositoryMock.Object,
          _unitOfWorkMock.Object,
          _passwordSecurityMock.Object);
    }

    [Fact]
    public async Task GetByCredentialsAsync_WithValidCredentials_ShouldReturnUser() {
      // Arrange
      string email = "test@test.com";
      string password = "password";
      string hashedPassword = "hashed_password";
      User user = new() { Id = 1, Email = email, PasswordHash = hashedPassword, Role = Role.Viewer };

      _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(email))
          .ReturnsAsync(true);
      _userRepositoryMock.Setup(r => r.GetByEmailAsync(email))
          .ReturnsAsync(user);
      _passwordSecurityMock.Setup(p => p.Verify(password, hashedPassword))
          .Returns(true);

      // Act
      User? result = await _userService.GetByCredentialsAsync(email, password);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(user, result);
    }

    [Fact]
    public async Task GetByCredentialsAsync_WithNonExistentUser_ShouldReturnNull() {
      // Arrange
      string email = "nonexistent@test.com";
      string password = "password";

      _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(email))
          .ReturnsAsync(false);

      // Act
      User? result = await _userService.GetByCredentialsAsync(email, password);

      // Assert
      Assert.Null(result);
      _userRepositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetByCredentialsAsync_WithInvalidPassword_ShouldReturnNull() {
      // Arrange
      string email = "test@test.com";
      string password = "wrong_password";
      string hashedPassword = "hashed_password";
      User user = new() { Id = 1, Email = email, PasswordHash = hashedPassword, Role = Role.Viewer };

      _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(email))
          .ReturnsAsync(true);
      _userRepositoryMock.Setup(r => r.GetByEmailAsync(email))
          .ReturnsAsync(user);
      _passwordSecurityMock.Setup(p => p.Verify(password, hashedPassword))
          .Returns(false);

      // Act
      User? result = await _userService.GetByCredentialsAsync(email, password);

      // Assert
      Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithNewUser_ShouldReturnUser() {
      // Arrange
      UserCreateData userCreateData = new("new@test.com", "password", Role.Viewer);
      string hashedPassword = "hashed_password";

      _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(userCreateData.Email))
          .ReturnsAsync(false);
      _passwordSecurityMock.Setup(p => p.Hash(userCreateData.Password))
          .Returns(hashedPassword);

      // Act
      User? result = await _userService.CreateAsync(userCreateData);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(userCreateData.Email, result.Email);
      Assert.Equal(hashedPassword, result.PasswordHash);
      _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == userCreateData.Email)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithExistingEmail_ShouldReturnNull() {
      // Arrange
      UserCreateData userCreateData = new("existing@test.com", "password", Role.Viewer);

      _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(userCreateData.Email))
          .ReturnsAsync(true);

      // Act
      User? result = await _userService.CreateAsync(userCreateData);

      // Assert
      Assert.Null(result);
      _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRoleByIdAsync_WithExistingUser_ShouldSucceed() {
      // Arrange
      long userId = 1L;
      User user = new() { Id = userId, Email = "test@test.com", PasswordHash = "hash", Role = Role.Viewer };
      UserRoleUpdateData updateData = new(Role.Admin);

      _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
      _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

      // Act
      OperationResult result = await _userService.UpdateRoleByIdAsync(userId, updateData);

      // Assert
      Assert.Equal(OperationResult.Success, result);
      Assert.Equal(updateData.Role, user.Role);
      _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithExistingUser_ShouldSucceed() {
      // Arrange
      long userId = 1L;
      User user = new() { Id = userId, Email = "test@test.com", PasswordHash = "hash", Role = Role.Viewer, Deleted = null};

      _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
      _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

      // Act
      OperationResult result = await _userService.DeleteByIdAsync(userId);

      // Assert
      Assert.Equal(OperationResult.Success, result);
      Assert.NotNull(user.Deleted);
      _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithNonExistentUser_ShouldReturnNotFound() {
      // Arrange
      long userId = 99L;
      _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

      // Act
      OperationResult result = await _userService.DeleteByIdAsync(userId);

      // Assert
      Assert.Equal(OperationResult.NotFound, result);
      _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
    }
  }
}