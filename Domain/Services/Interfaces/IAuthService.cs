namespace Domain.Services.Interfaces;

using Entities;

public interface IAuthService {
  Task<bool> RegisterAsync(User data);
}
