namespace Domain.Repositories;

using Entities;

public interface IVerificationTokenRepository {
  Task<VerificationToken?> GetByIdAsync(long id);
  Task<VerificationToken?> GetByTokenAsync(string token);
  Task<List<VerificationToken>> GetAllInactiveAsync();
  Task AddAsync(VerificationToken token);
  void Remove(VerificationToken token);
}
