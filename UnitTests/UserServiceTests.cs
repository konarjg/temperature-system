using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Util;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.External;
using Moq;
using Xunit;
using System.Collections.Generic;
using System;

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
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = new User {
            Id = 1,
            Email = "test@test.com",
            PasswordHash = "abcdadwea",
            Role = Role.Unverified
        };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _userService.GetByIdAsync(1);

        // Assert
        Assert.Equal(user, result);
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

    [Fact]
    public async Task DeleteAllInactiveUsersAsync_ShouldRemoveInactiveUsers()
    {
        // Arrange
        var inactiveUsers = new List<User> { new User {
            Id = 1,
            Email = "test1@test.com",
            PasswordHash = "adavavaw311",
            Role = Role.Unverified
        }, new User {
                Id = 2,
                Email = "test2@test.com",
                PasswordHash = "bawdwadwada",
                Role = Role.Unverified
            }
        };
        _userRepositoryMock.Setup(r => r.GetAllInactiveAsync()).ReturnsAsync(inactiveUsers);
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(inactiveUsers.Count);

        // Act
        var result = await _userService.DeleteAllInactiveUsersAsync();

        // Assert
        Assert.True(result);
        _userRepositoryMock.Verify(r => r.Remove(It.IsAny<User>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUserData_WhenUserExists()
    {
        // Arrange
        var existingUser = new User { Id = 1, Email = "old@test.com", PasswordHash = "old_hash", Role = Role.Viewer };
        var newData = new User { Email = "new@test.com", PasswordHash = "new_password", Role = Role.Admin };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUser);
        _passwordSecurityMock.Setup(p => p.Hash("new_password")).Returns("new_hash");
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _userService.UpdateAsync(1, newData);

        // Assert
        Assert.True(result);
        Assert.Equal("new@test.com", existingUser.Email);
        Assert.Equal("new_hash", existingUser.PasswordHash);
        Assert.Equal(Role.Admin, existingUser.Role);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSetDeletedDate()
    {
        // Arrange
        var user = new User {
            Id = 1,
            Email = "test@test.com",
            PasswordHash = "adavwafwg",
            Role = Role.Unverified
        };
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _userService.DeleteAsync(user);

        // Assert
        Assert.True(result);
        Assert.NotNull(user.Deleted);
    }
}