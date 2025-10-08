namespace DatabaseAdapters.Repositories;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class VerificationTokenRepository(IDatabaseContext databaseContext) : IVerificationTokenRepository {

  public async Task<VerificationToken?> GetByIdAsync(long id) {
    return await databaseContext.VerificationTokens.Include(v => v.User).FirstOrDefaultAsync(v => v.Id == id);
  }

  public async Task<VerificationToken?> GetByTokenAsync(string token) {
    return await databaseContext.VerificationTokens.Include(v => v.User).FirstOrDefaultAsync(v => v.Token.Equals(token));
  }

  public async Task<List<VerificationToken>> GetAllInactiveAsync() {
    return await databaseContext.VerificationTokens.Where(v => v.Revoked != null || v.Expires < DateTime.UtcNow).ToListAsync();
  }

  public async Task AddAsync(VerificationToken token) {
    await databaseContext.VerificationTokens.AddAsync(token);
  }
  
  public void Remove(VerificationToken token) {
    databaseContext.VerificationTokens.Remove(token);
  }
}
