using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Util;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.External;
using Moq;
using Xunit;

namespace UnitTests;

public class UserServiceTests {
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordSecurity> _passwordSecurityMock;
    private readonly UserService _userService;

    public UserServiceTests() {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordSecurityMock = new Mock<IPasswordSecurity>();
        _userService = new UserService(_userRepositoryMock.Object, _unitOfWorkMock.Object, _passwordSecurityMock.Object);
    }

    [Fact]
    public async Task GetByCredentialsAsync_ShouldReturnUser_WhenCredentialsAreValid() {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com", PasswordHash = "hashed_password", Role = Role.Viewer };
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync("test@test.com")).ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);
        _passwordSecurityMock.Setup(p => p.Verify("password", "hashed_password")).Returns(true);

        // Act
        var result = await _userService.GetByCredentialsAsync("test@test.com", "password");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user, result);
    }

    [Fact]
    public async Task GetByCredentialsAsync_ShouldReturnNull_WhenPasswordIsInvalid() {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com", PasswordHash = "hashed_password", Role = Role.Viewer };
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync("test@test.com")).ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);
        _passwordSecurityMock.Setup(p => p.Verify("wrong_password", "hashed_password")).Returns(false);

        // Act
        var result = await _userService.GetByCredentialsAsync("test@test.com", "wrong_password");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldHashPasswordAndAddUser() {
        // Arrange
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Unverified };
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(user.Email)).ReturnsAsync(false);
        _passwordSecurityMock.Setup(p => p.Hash("password")).Returns("hashed_password");

        // Act
        var result = await _userService.CreateAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("hashed_password", result.PasswordHash);
        _userRepositoryMock.Verify(r => r.AddAsync(user), Times.Once);
        _passwordSecurityMock.Verify(p => p.Hash("password"), Times.Once);
    }
}