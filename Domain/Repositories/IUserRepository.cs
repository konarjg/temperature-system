namespace Domain.Repositories;

using Entities;

public interface IUserRepository {
  Task<User?> GetByIdAsync(long id);
  Task<bool> ExistsByEmailAsync(string email);
  Task<User?> GetByEmailAsync(string email);
  Task<List<User>> GetAllInactiveAsync();
  Task AddAsync(User user);
  void Remove(User user);
}
