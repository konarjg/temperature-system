namespace Domain.Services.External;

using Util;

public interface IEmailSettingsProvider {
  string VerificationUrl { get; }
  string SmtpHost { get; }
  int SmtpPort { get; }
  string SenderEmail { get; }
  string SenderPassword { get; }
}
