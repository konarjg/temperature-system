namespace DatabaseAdapters;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IDatabaseContext {
  DbSet<User> Users { get; set; }
  DbSet<RefreshToken> RefreshTokens { get; set; }
  DbSet<VerificationToken> VerificationTokens { get; set; }
  DbSet<Measurement> Measurements { get; set; }
  DbSet<Sensor> Sensors { get; set; }

  Task<int> SaveChangesAsync();
}
