namespace Domain.Services.External;

using Util;

public interface IEmailSettingsProvider {
  string GetVerificationUrl();
}
