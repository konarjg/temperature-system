using System;
using System.Threading.Tasks;
using DatabaseAdapters.Repositories;
using Domain.Entities;
using Domain.Entities.Util;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests
{
    public class VerificationTokenRepositoryTests : BaseServiceTests
    {
        private readonly VerificationTokenRepository _repository;

        public VerificationTokenRepositoryTests()
        {
            _repository = new VerificationTokenRepository(DbContext);
        }

        [Fact]
        public async Task GetByTokenAsync_ShouldReturnToken_WhenTokenExists()
        {
            // Arrange
            var user = new User { Email = "test@test.com", PasswordHash = "hash", Role = Domain.Entities.Util.Role.Viewer };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var verificationToken = new VerificationToken { Token = "token", Expires = DateTime.UtcNow.AddDays(1), User = user };
            DbContext.VerificationTokens.Add(verificationToken);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByTokenAsync("token");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(verificationToken.Id, result.Id);
        }
    }
}