namespace Domain.Services.Interfaces;

using Entities;

public interface IUserService {
  Task<User?> GetByIdAsync(long id);
  Task<User?> GetByCredentialsAsync(string email,
    string password);
  Task<User?> CreateAsync(User data);
  Task<bool> DeleteAllInactiveUsersAsync();
  Task<bool> UpdateAsync(long id, User data);
  Task<bool> DeleteAsync(User user);
}
