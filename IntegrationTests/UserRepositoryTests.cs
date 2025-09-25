using System;
using System.Threading.Tasks;
using DatabaseAdapters.Repositories;
using Domain.Entities;
using Domain.Entities.Util;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests
{
    public class UserRepositoryTests : BaseServiceTests
    {
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            _repository = new UserRepository(DbContext);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User { Email = "test@test.com", PasswordHash = "hash", Role = Domain.Entities.Util.Role.Viewer };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByEmailAsync("test@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
        }
    }
}