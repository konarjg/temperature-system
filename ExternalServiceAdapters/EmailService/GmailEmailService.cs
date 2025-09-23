namespace ExternalServiceAdapters.EmailService;

using Domain.Services.External;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

public class GmailEmailService(ILogger<GmailEmailService> logger, IEmailSettingsProvider settings) : IEmailService {
  
  
  public async Task<bool> SendEmail(string subject,
    string body,
    string to) {
    try
    {
      var emailMessage = new MimeMessage();
      emailMessage.From.Add(MailboxAddress.Parse(settings.SenderEmail));
      emailMessage.To.Add(MailboxAddress.Parse(to));
      emailMessage.Subject = subject;
      emailMessage.Body = new TextPart(TextFormat.Html) { Text = body }; 

      using var smtpClient = new SmtpClient();

      await smtpClient.ConnectAsync(settings.SmtpHost, settings.SmtpPort, SecureSocketOptions.StartTls);
      await smtpClient.AuthenticateAsync(settings.SenderEmail, settings.SenderPassword);
      await smtpClient.SendAsync(emailMessage);
      await smtpClient.DisconnectAsync(true);

      logger.LogInformation("Email successfully sent to {Recipient}", to);
      return true;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to send email to {Recipient}", to);
      return false;
    }
  }
}
