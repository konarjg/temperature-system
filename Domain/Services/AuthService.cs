namespace Domain.Services;

using Entities;
using External;
using Interfaces;
using Repositories;

public class AuthService(ITokenGenerator tokenGenerator, IVerificationTokenRepository verificationTokenRepository, IPasswordSecurity passwordSecurity, IUserService userService, IUnitOfWork unitOfWork, IEmailService emailService, IEmailSettingsProvider emailSettingsProvider) : IAuthService{
  private const string VerificationEmailSubject = "Temperature System email verification";
  private const string VerificationEmailBodyFormat = "Thank you for choosing Temperature System, click this link to verify your account {0}/{1}";
  
  public async Task<bool> RegisterAsync(User data) {
    User? user = await userService.CreateAsync(data);

    if (user == null) {
      return false;
    }

    VerificationToken token = tokenGenerator.GenerateVerificationToken(user);
    verificationTokenRepository.AddAsync(token);

    if (await unitOfWork.CompleteAsync() <= 0) {
      return false;
    }

    string body = string.Format(VerificationEmailBodyFormat,emailSettingsProvider.GetVerificationUrl(),token.Token);
    return emailService.SendEmail(VerificationEmailSubject, body, user.Email);
  }
}
