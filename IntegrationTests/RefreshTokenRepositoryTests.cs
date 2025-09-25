using System;
using System.Threading.Tasks;
using DatabaseAdapters.Repositories;
using Domain.Entities;
using Domain.Entities.Util;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests
{
    public class RefreshTokenRepositoryTests : BaseServiceTests
    {
        private readonly RefreshTokenRepository _repository;

        public RefreshTokenRepositoryTests()
        {
            _repository = new RefreshTokenRepository(DbContext);
        }

        [Fact]
        public async Task GetByTokenAsync_ShouldReturnToken_WhenTokenExists()
        {
            // Arrange
            var user = new User { Email = "test@test.com", PasswordHash = "hash", Role = Domain.Entities.Util.Role.Viewer };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var refreshToken = new RefreshToken { Token = "token", Expires = DateTime.UtcNow.AddDays(1), User = user };
            DbContext.RefreshTokens.Add(refreshToken);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByTokenAsync("token");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(refreshToken.Id, result.Id);
        }
    }
}