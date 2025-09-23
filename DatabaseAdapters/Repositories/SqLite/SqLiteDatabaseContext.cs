namespace DatabaseAdapters.Repositories.SqLite;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class SqLiteDatabaseContext(DbContextOptions<SqLiteDatabaseContext> options) : DbContext(options),IDatabaseContext {
  public DbSet<User> Users { get; set; }
  public DbSet<RefreshToken> RefreshTokens { get; set; }
  public DbSet<VerificationToken> VerificationTokens { get; set; }
  public DbSet<Measurement> Measurements { get; set; }
  public DbSet<Sensor> Sensors { get; set; }

  public async Task<int> SaveChangesAsync() {
    return await base.SaveChangesAsync();
  }
}
