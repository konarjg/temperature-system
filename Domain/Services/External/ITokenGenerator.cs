namespace Domain.Services.External;

using Entities;

public interface ITokenGenerator {
  VerificationToken GenerateVerificationToken(User user);
  RefreshToken GenerateRefreshToken(User user);
  string GenerateAccessToken(User user);
}
