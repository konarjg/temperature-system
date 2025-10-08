namespace Domain.Services;

using Entities;
using External;
using Interfaces;
using Mappers;
using Records;
using Repositories;
using Util;

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

  public async Task<User?> CreateAsync(UserCreateData data) {
    if (await userRepository.ExistsByEmailAsync(data.Email)) {
      return null;
    }

    User user = data.ToEntity(passwordSecurity);
    await userRepository.AddAsync(user);

    return user;
  }

  public async Task<bool> DeleteAllInactiveUsersAsync() {
    List<User> inactiveUsers = await userRepository.GetAllInactiveAsync();

    foreach (User user in inactiveUsers) {
      userRepository.Remove(user);
    }
    
    return await unitOfWork.CompleteAsync() != 0;
  }

  public async Task<OperationResult> UpdateRoleByIdAsync(long id,
    UserRoleUpdateData data) {
    
    User? user = await userRepository.GetByIdAsync(id);

    if (user == null) {
      return OperationResult.NotFound;
    }
    
    user.UpdateRole(data);
    
    return await unitOfWork.CompleteAsync() != 0 ? OperationResult.Success : OperationResult.ServerError;
  }

  public async Task<OperationResult> UpdateCredentialsByIdAsync(long id,
    UserCredentialsUpdateData data) {

    User? user = await userRepository.GetByIdAsync(id);

    if (user == null) {
      return OperationResult.NotFound;
    }
    
    user.UpdateCredentials(data, passwordSecurity);
    
    return await unitOfWork.CompleteAsync() != 0 ? OperationResult.Success : OperationResult.ServerError;
  }

  public async Task<OperationResult> DeleteByIdAsync(long id) {
    User? user = await userRepository.GetByIdAsync(id);

    if (user == null) {
      return OperationResult.NotFound;
    }
    
    user.Deleted = DateTime.UtcNow;
    return await unitOfWork.CompleteAsync() != 0 ? OperationResult.Success : OperationResult.ServerError;
  }
}
