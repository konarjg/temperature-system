namespace Domain.Repositories;

using Entities;

public interface IRefreshTokenRepository {
  Task<RefreshToken?> GetByIdAsync(long id);
  Task<RefreshToken?> GetByTokenAsync(string token);
  Task<List<RefreshToken>> GetAllInactiveAsync();
  Task AddAsync(RefreshToken token);
  void Remove(RefreshToken token);
}
