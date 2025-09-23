namespace DatabaseAdapters.Repositories.Test;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class TestDatabaseContext(DbContextOptions<TestDatabaseContext> options) : DbContext(options), IDatabaseContext{

  public DbSet<User> Users { get; set; }
  public DbSet<RefreshToken> RefreshTokens { get; set; }
  public DbSet<VerificationToken> VerificationTokens { get; set; }
  public DbSet<Measurement> Measurements { get; set; }
  public DbSet<Sensor> Sensors { get; set; }

  public async Task<int> SaveChangesAsync() {
    return await base.SaveChangesAsync();
  }
}
