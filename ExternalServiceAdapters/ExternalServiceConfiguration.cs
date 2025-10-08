namespace ExternalServiceAdapters;

using Domain.Services.External;
using EmailService;
using EmailSettingsProvider;
using Microsoft.Extensions.DependencyInjection;
using NotificationService;
using PasswordSecurity;
using TemperatureSensorReader;
using TokenGenerator;

public static class ExternalServiceConfiguration {
  public static IServiceCollection AddExternalServices(this IServiceCollection services) {
    services.AddScoped<IEmailService,MockEmailService>();
    services.AddScoped<IEmailSettingsProvider,ConfigurationEmailSettingsProvider>();
    services.AddScoped<IPasswordSecurity,Argon2PasswordSecurity>();
    services.AddScoped<ITokenGenerator,JwtTokenGenerator>();
    services.AddScoped<ITemperatureSensorReader,FakeTemperatureSensorReader>();
    services.AddNotificationServices();
    
    return services;
  }
}
