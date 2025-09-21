using System;
using System.Collections.Generic;
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
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists() {
        var user = new User { Id = 1, Email = "test@test.com", PasswordHash = "hashed_password", Role = Role.Viewer };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _userService.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist() {
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User)null);

        var result = await _userService.GetByIdAsync(1);

        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByCredentialsAsync_ShouldReturnUser_WhenCredentialsAreValid() {
        var user = new User { Id = 1, Email = "test@test.com", PasswordHash = "hashed_password", Role = Role.Viewer };
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync("test@test.com")).ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);
        _passwordSecurityMock.Setup(p => p.Verify("password", "hashed_password")).Returns(true);

        var result = await _userService.GetByCredentialsAsync("test@test.com", "password");

        Assert.NotNull(result);
        Assert.Equal(user, result);
    }

    [Fact]
    public async Task GetByCredentialsAsync_ShouldReturnNull_WhenEmailDoesNotExist() {
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync("test@test.com")).ReturnsAsync(false);

        var result = await _userService.GetByCredentialsAsync("test@test.com", "password");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByCredentialsAsync_ShouldReturnNull_WhenPasswordIsInvalid() {
        var user = new User { Id = 1, Email = "test@test.com", PasswordHash = "hashed_password", Role = Role.Viewer };
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync("test@test.com")).ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);
        _passwordSecurityMock.Setup(p => p.Verify("wrong_password", "hashed_password")).Returns(false);

        var result = await _userService.GetByCredentialsAsync("test@test.com", "wrong_password");

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnUser_WhenCreationIsSuccessful() {
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Unverified };
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(user.Email)).ReturnsAsync(false);
        _passwordSecurityMock.Setup(p => p.Hash("password")).Returns("hashed_password");

        var result = await _userService.CreateAsync(user);

        Assert.NotNull(result);
        Assert.Equal("hashed_password", result.PasswordHash);
        _userRepositoryMock.Verify(r => r.AddAsync(user), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenEmailAlreadyExists() {
        var user = new User { Email = "test@test.com", PasswordHash = "password", Role = Role.Unverified };
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(user.Email)).ReturnsAsync(true);

        var result = await _userService.CreateAsync(user);

        Assert.Null(result);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAllInactiveUsersAsync_ShouldReturnTrue_WhenDeletionIsSuccessful() {
        var inactiveUsers = new List<User> { 
            new User { Id = 1, Email = "a@a.com", PasswordHash = "p", Role = Role.Unverified }, 
            new User { Id = 2, Email = "b@b.com", PasswordHash = "p", Role = Role.Viewer, Deleted = DateTime.UtcNow } 
        };
        _userRepositoryMock.Setup(r => r.GetAllInactiveAsync()).ReturnsAsync(inactiveUsers);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(2);

        var result = await _userService.DeleteAllInactiveUsersAsync();

        Assert.True(result);
        _userRepositoryMock.Verify(r => r.Remove(It.IsAny<User>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenUpdateIsSuccessful() {
        var existingUser = new User { Id = 1, Email = "old@test.com", PasswordHash = "old_hash", Role = Role.Viewer };
        var updatedData = new User { Email = "new@test.com", PasswordHash = "new_password", Role = Role.Admin };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUser);
        _passwordSecurityMock.Setup(p => p.Hash("new_password")).Returns("new_hash");
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var result = await _userService.UpdateAsync(1, updatedData);

        Assert.True(result);
        Assert.Equal("new@test.com", existingUser.Email);
        Assert.Equal("new_hash", existingUser.PasswordHash);
        Assert.Equal(Role.Admin, existingUser.Role);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenUserDoesNotExist() {
        var updatedData = new User { Email = "new@test.com", PasswordHash = "new_password", Role = Role.Admin };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User)null);

        var result = await _userService.UpdateAsync(1, updatedData);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenSoftDeleteIsSuccessful() {
        var user = new User { Id = 1, Email = "test@test.com", PasswordHash = "password", Role = Role.Viewer };
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var result = await _userService.DeleteAsync(user);

        Assert.True(result);
        Assert.NotNull(user.Deleted);
    }
}