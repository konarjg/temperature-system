using Domain.Entities;
using Domain.Records;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.External;
using Moq;

namespace UnitTests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordSecurity> _passwordSecurityMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordSecurityMock = new Mock<IPasswordSecurity>();

        _userService = new UserService(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _passwordSecurityMock.Object);
    }

    [Fact]
    public async Task GetByCredentialsAsync_WithValidCredentials_ShouldReturnUser()
    {
        // Arrange
        var email = "test@test.com";
        var password = "password";
        var hashedPassword = "hashed_password";
        var user = new User { Id = 1, Email = email, PasswordHash = hashedPassword, Role = Domain.Entities.Util.Role.Viewer };

        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(email))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);
        _passwordSecurityMock.Setup(p => p.Verify(password, hashedPassword))
            .Returns(true);

        // Act
        var result = await _userService.GetByCredentialsAsync(email, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user, result);
    }

    [Fact]
    public async Task GetByCredentialsAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var email = "nonexistent@test.com";
        var password = "password";

        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(email))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.GetByCredentialsAsync(email, password);

        // Assert
        Assert.Null(result);
        _userRepositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetByCredentialsAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var email = "test@test.com";
        var password = "wrong_password";
        var hashedPassword = "hashed_password";
        var user = new User { Id = 1, Email = email, PasswordHash = hashedPassword, Role = Domain.Entities.Util.Role.Viewer };

        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(email))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);
        _passwordSecurityMock.Setup(p => p.Verify(password, hashedPassword))
            .Returns(false); // Simulate password verification failure

        // Act
        var result = await _userService.GetByCredentialsAsync(email, password);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithNewUser_ShouldReturnUser()
    {
        // Arrange
        var userCreateData = new UserCreateData("new@test.com", "password", Domain.Entities.Util.Role.Viewer);
        var hashedPassword = "hashed_password";

        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(userCreateData.Email))
            .ReturnsAsync(false);
        _passwordSecurityMock.Setup(p => p.Hash(userCreateData.Password))
            .Returns(hashedPassword);

        // Act
        var result = await _userService.CreateAsync(userCreateData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userCreateData.Email, result.Email);
        Assert.Equal(hashedPassword, result.PasswordHash);
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == userCreateData.Email)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithExistingEmail_ShouldReturnNull()
    {
        // Arrange
        var userCreateData = new UserCreateData("existing@test.com", "password", Domain.Entities.Util.Role.Viewer);

        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(userCreateData.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.CreateAsync(userCreateData);

        // Assert
        Assert.Null(result);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRoleByIdAsync_WithExistingUser_ShouldSucceed()
    {
        // Arrange
        var userId = 1L;
        var user = new User { Id = userId, Email = "test@test.com", PasswordHash = "hash", Role = Domain.Entities.Util.Role.Viewer };
        var updateData = new UserRoleUpdateData(Domain.Entities.Util.Role.Admin);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _userService.UpdateRoleByIdAsync(userId, updateData);

        // Assert
        Assert.Equal(Domain.Services.Util.OperationResult.Success, result);
        Assert.Equal(updateData.Role, user.Role);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithExistingUser_ShouldSucceed()
    {
        // Arrange
        var userId = 1L;
        var user = new User { Id = userId, Email = "test@test.com", PasswordHash = "hash", Role = Domain.Entities.Util.Role.Viewer, Deleted = null};

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _userService.DeleteByIdAsync(userId);

        // Assert
        Assert.Equal(Domain.Services.Util.OperationResult.Success, result);
        Assert.NotNull(user.Deleted);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        var userId = 99L;
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null);

        // Act
        var result = await _userService.DeleteByIdAsync(userId);

        // Assert
        Assert.Equal(Domain.Services.Util.OperationResult.NotFound, result);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
    }
}