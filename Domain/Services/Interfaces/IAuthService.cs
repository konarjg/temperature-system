namespace Domain.Services.Interfaces;

using Entities;
using Records;
using Util;

public interface IAuthService {
  Task<AuthResult?> LoginAsync(string email,
    string password);
  Task<AuthResult?> RefreshAsync(string token);
  Task<bool> LogoutAsync(string token);
  Task<RegisterResult> RegisterAsync(UserCreateData data);
  Task<OperationResult> VerifyAsync(string token);
}
