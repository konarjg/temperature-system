namespace Domain.Security;

using Entities;

public interface ITokenGenerator {
  RefreshToken GenerateRefreshToken(User user);
  VerificationToken GenerateVerificationToken(User user);
  string GenerateAccessToken(User user);
}
