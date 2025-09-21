namespace Domain.Services;

using Entities;
using External;
using Interfaces;
using Repositories;

public class UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordSecurity passwordSecurity) : IUserService{

  public async Task<User?> GetByIdAsync(long id) {
    return await userRepository.GetByIdAsync(id);
  }
  public async Task<User?> GetByCredentialsAsync(string email,
    string password) {

    if (!await userRepository.ExistsByEmailAsync(email)) {
      return null;
    }

    User? user = await userRepository.GetByEmailAsync(email);

    if (!passwordSecurity.Verify(password,user.PasswordHash)) {
      return null;
    }

    return user;
  }

  public async Task<User?> CreateAsync(User data) {
    if (await userRepository.ExistsByEmailAsync(data.Email)) {
      return null;
    }

    data.PasswordHash = passwordSecurity.Hash(data.PasswordHash);
    await userRepository.AddAsync(data);

    return data;
  }

  public async Task<bool> DeleteAllInactiveUsersAsync() {
    List<User> inactiveUsers = await userRepository.GetAllInactiveAsync();

    foreach (User user in inactiveUsers) {
      userRepository.Remove(user);
    }
    
    return await unitOfWork.CompleteAsync() != 0;
  }

  public async Task<bool> UpdateAsync(long id,
    User data) {

    User? user = await userRepository.GetByIdAsync(id);

    if (user == null) {
      return false;
    }
    
    user.Email = data.Email;
    user.PasswordHash = passwordSecurity.Hash(data.PasswordHash);
    user.Role = data.Role;
    
    return await unitOfWork.CompleteAsync() != 0;
  }

  public async Task<bool> DeleteAsync(User user) {
    user.Deleted = DateTime.UtcNow;
    return await unitOfWork.CompleteAsync() != 0;
  }
}
