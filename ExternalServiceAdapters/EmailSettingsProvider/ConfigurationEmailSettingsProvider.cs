namespace ExternalServiceAdapters.EmailSettingsProvider;

using Domain.Services.External;
using Microsoft.Extensions.Configuration;

public class ConfigurationEmailSettingsProvider(IConfiguration configuration) : IEmailSettingsProvider{
  public string VerificationUrl =>  configuration["Email:VerificationUrl"] ?? "http://localhost:8080/verify";
  public string SmtpHost => configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
  public int SmtpPort => int.Parse(configuration["Email:SmtpPort"] ?? "587");
  public string SenderEmail => configuration["Email:SenderEmail"] ?? "temperaturesystem@gmail.com";
  public string SenderPassword => configuration["Email:SenderPassword"] ?? "temperaturesystem";
}
