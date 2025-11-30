namespace ExternalServiceAdapters.EmailService;

using System.Linq;
using Domain.Services.External;
using Microsoft.Extensions.Logging;

public class MockEmailService(ILogger<MockEmailService> logger) : IEmailService {

  public string? LastVerificationToken { get; private set; }

  public Task<bool> SendEmail(string subject,
    string body,
    string to) {
    
    logger.LogInformation($"Subject: {subject}\nBody: {body}");

    if (body.Contains("verify your account"))
    {
      string urlPath = body.Split(' ').Last();
      string? token = urlPath.Split('/').Last();
      LastVerificationToken = token;
    }

    return Task.FromResult(true);
  }
}
