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

public class UserServiceTests : BaseServiceTests {
    private readonly IUserService _userService;

    public UserServiceTests() {
        _userService = ServiceProvider.GetRequiredService<IUserService>();
        var passwordSecurityMock = Mock.Get(ServiceProvider.GetRequiredService<IPasswordSecurity>());

        passwordSecurityMock.Setup(p => p.Hash(It.IsAny<string>())).Returns((string s) => "hashed_" + s);
        passwordSecurityMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string pass, string hash) => hash == "hashed_" + pass);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddUserToDatabase() {
        // Arrange
        var user = new User { Email = "test@example.com", PasswordHash = "password", Role = Role.Unverified };

        // Act: Create the user. Note: In a real app, this would likely be part of a larger transaction.
        var createdUser = await _userService.CreateAsync(user);
        // Manually commit the transaction for the purpose of this test.
        await DbContext.SaveChangesAsync();

        // Assert
        Assert.NotNull(createdUser);
        var savedUser = await _userService.GetByIdAsync(createdUser.Id);
        Assert.NotNull(savedUser);
        Assert.Equal("test@example.com", savedUser.Email);
        Assert.Equal("hashed_password", savedUser.PasswordHash); // Verify password was hashed
    }

    [Fact]
    public async Task GetByCredentialsAsync_ShouldReturnUser_WhenCredentialsAreCorrect() {
        // Arrange
        var user = new User { Email = "correct@example.com", PasswordHash = "hashed_password", Role = Role.Viewer };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        var foundUser = await _userService.GetByCredentialsAsync("correct@example.com", "password");

        // Assert
        Assert.NotNull(foundUser);
        Assert.Equal(user.Id, foundUser.Id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldPerformSoftDelete() {
        // Arrange
        var user = new User { Email = "delete@me.com", PasswordHash = "hashed_password", Role = Role.Viewer };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _userService.DeleteAsync(user);

        // Assert
        Assert.True(result);
        var savedUser = await DbContext.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.NotNull(savedUser.Deleted);
    }
}