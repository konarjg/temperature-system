namespace ExternalServiceAdapters;

using Domain.Services.External;
using EmailService;
using EmailSettingsProvider;
using Microsoft.Extensions.DependencyInjection;
using PasswordSecurity;
using TemperatureSensorReader;
using TokenGenerator;

public static class ExternalServiceConfiguration {
  public static IServiceCollection AddExternalServices(this IServiceCollection services) {
    services.AddScoped<IEmailService,GmailEmailService>();
    services.AddScoped<IEmailSettingsProvider,ConfigurationEmailSettingsProvider>();
    services.AddScoped<IPasswordSecurity,Argon2PasswordSecurity>();
    services.AddScoped<ITokenGenerator,JwtTokenGenerator>();
    services.AddScoped<ITemperatureSensorReader,Ds18B20TemperatureSensorReader>();
    
    return services;
  }
}
