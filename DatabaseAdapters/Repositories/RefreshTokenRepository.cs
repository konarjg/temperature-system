namespace DatabaseAdapters.Repositories;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class RefreshTokenRepository(IDatabaseContext databaseContext) : IRefreshTokenRepository {

  public async Task<RefreshToken?> GetByIdAsync(long id) {
    return await databaseContext.RefreshTokens.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
  }

  public async Task<RefreshToken?> GetByTokenAsync(string token) {
    return await databaseContext.RefreshTokens.Include(r => r.User).FirstOrDefaultAsync(r => r.Token.Equals(token));
  }

  public async Task<List<RefreshToken>> GetAllInactiveAsync() {
    return await databaseContext.RefreshTokens.Where(r => r.Revoked != null || r.Expires < DateTime.UtcNow).ToListAsync();
  }
  
  public async Task AddAsync(RefreshToken token) {
    await databaseContext.RefreshTokens.AddAsync(token);
  }
  
  public void Remove(RefreshToken token) {
    databaseContext.RefreshTokens.Remove(token);
  }
}
