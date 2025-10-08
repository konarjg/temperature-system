namespace Domain.Services;

using Entities;
using Entities.Util;
using External;
using Interfaces;
using Records;
using Repositories;
using Util;

public class AuthService(ITokenGenerator tokenGenerator, IVerificationTokenRepository verificationTokenRepository, IRefreshTokenRepository refreshTokenRepository, IUserService userService, IUnitOfWork unitOfWork, IEmailService emailService, IEmailSettingsProvider emailSettingsProvider) : IAuthService{
  private const string VerificationEmailSubject = "Temperature System email verification";
  private const string VerificationEmailBodyFormat = "Thank you for choosing Temperature System, click this link to verify your account {0}/{1}";

  public async Task<AuthResult?> LoginAsync(string email,
    string password) {

    User? user = await userService.GetByCredentialsAsync(email,password);

    if (user == null || !user.IsActive) {
      return null;
    }

    RefreshToken refreshToken = tokenGenerator.GenerateRefreshToken(user);
    await refreshTokenRepository.AddAsync(refreshToken);
    
    if (await unitOfWork.CompleteAsync() <= 0) {
      return null;
    }
    
    string accessToken = tokenGenerator.GenerateAccessToken(user);

    return new AuthResult(user,refreshToken,accessToken);
  }

  public async Task<AuthResult?> RefreshAsync(string token) {
    RefreshToken? refreshToken = await refreshTokenRepository.GetByTokenAsync(token);

    if (refreshToken == null || !refreshToken.IsActive) {
      return null;
    }

    User user = refreshToken.User;
    
    refreshToken.Revoked = DateTime.UtcNow;
    RefreshToken newToken = tokenGenerator.GenerateRefreshToken(user);
    await refreshTokenRepository.AddAsync(newToken);

    if (await unitOfWork.CompleteAsync() <= 0) {
      return null;
    }
    
    string accessToken = tokenGenerator.GenerateAccessToken(user);

    return new AuthResult(user,newToken,accessToken);
  }

  public async Task<bool> LogoutAsync(string token) {
    RefreshToken? refreshToken = await refreshTokenRepository.GetByTokenAsync(token);

    if (refreshToken == null || !refreshToken.IsActive) {
      return false;
    }

    refreshToken.Revoked = DateTime.UtcNow;
    return await unitOfWork.CompleteAsync() != 0;
  }
  
  public async Task<RegisterResult> RegisterAsync(UserCreateData data) {
    User? user = await userService.CreateAsync(data);

    if (user == null) {
      return new RegisterResult(null, RegisterState.Conflict);
    }

    VerificationToken token = tokenGenerator.GenerateVerificationToken(user);
    verificationTokenRepository.AddAsync(token);

    if (await unitOfWork.CompleteAsync() <= 0) {
      return new RegisterResult(null, RegisterState.ServerError);
    }

    string body = string.Format(VerificationEmailBodyFormat,emailSettingsProvider.VerificationUrl,token.Token);
    return await emailService.SendEmail(VerificationEmailSubject, body, user.Email) ? new RegisterResult(user, RegisterState.Success) : new RegisterResult(null, RegisterState.ServerError);
  }
  
  public async Task<OperationResult> VerifyAsync(string token) {
    VerificationToken? verificationToken = await verificationTokenRepository.GetByTokenAsync(token);

    if (verificationToken == null) {
      return OperationResult.NotFound;
    }

    User user = verificationToken.User;
    verificationToken.Revoked = DateTime.UtcNow;
    await unitOfWork.CompleteAsync();
    
    return await userService.UpdateRoleByIdAsync(user.Id,new UserRoleUpdateData(Role.Viewer));
  }
}
