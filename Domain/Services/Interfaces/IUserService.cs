namespace Domain.Services.Interfaces;

using Entities;
using Records;
using Util;

public interface IUserService {
  Task<User?> GetByIdAsync(long id);
  Task<User?> GetByCredentialsAsync(string email,
    string password);
  Task<User?> CreateAsync(UserCreateData data);
  Task<bool> DeleteAllInactiveUsersAsync();
  Task<OperationResult> UpdateCredentialsByIdAsync(long id, UserCredentialsUpdateData data);
  Task<OperationResult> UpdateRoleByIdAsync(long id, UserRoleUpdateData data);
  Task<OperationResult> DeleteByIdAsync(long id);
}
