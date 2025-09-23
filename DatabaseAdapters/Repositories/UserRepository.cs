namespace DatabaseAdapters.Repositories;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class UserRepository(IDatabaseContext databaseContext) : IUserRepository {

  public async Task<User?> GetByIdAsync(long id) {
    return await databaseContext.Users.FindAsync(id);
  }

  public async Task<bool> ExistsByEmailAsync(string email) {
    return await databaseContext.Users.AnyAsync(u => u.Email.Equals(email));
  }

  public async Task<User?> GetByEmailAsync(string email) {
    return await databaseContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(email));
  }

  public async Task<List<User>> GetAllInactiveAsync() {
    return await databaseContext.Users.Where(u => !u.IsActive).ToListAsync();
  }

  public async Task AddAsync(User user) {
    await databaseContext.Users.AddAsync(user);
  }
  
  public void Remove(User user) {
    databaseContext.Users.Remove(user);
  }
}
