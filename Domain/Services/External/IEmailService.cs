namespace Domain.Services.External;

using Entities;

public interface IEmailService {
  Task<bool> SendEmail(string subject, string body, string to);
}
