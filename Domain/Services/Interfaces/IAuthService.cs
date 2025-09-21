namespace Domain.Services.Interfaces;

using Entities;
using Records;

public interface IAuthService {
  Task<AuthResult?> LoginAsync(string email,
    string password);
  Task<AuthResult?> RefreshAsync(string token);
  Task<bool> LogoutAsync(string token);
  Task<bool> RegisterAsync(User data);
  Task<bool> VerifyAsync(string token);
}
