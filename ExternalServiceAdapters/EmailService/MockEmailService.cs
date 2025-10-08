namespace ExternalServiceAdapters.EmailService;

using Domain.Services.External;
using Microsoft.Extensions.Logging;

public class MockEmailService(ILogger<MockEmailService> logger) : IEmailService {
  public Task<bool> SendEmail(string subject,
    string body,
    string to) {
    
    logger.LogInformation($"Subject: {subject}\nBody: {body}");
    return Task.FromResult(true);
  }
}
