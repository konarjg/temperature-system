namespace Domain.Services.External;

using Entities;

public interface IEmailService {
  bool SendEmail(string subject, string body, string to);
}
