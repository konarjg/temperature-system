namespace Domain;

using Microsoft.Extensions.DependencyInjection;
using Services;
using Services.Interfaces;

public static class DomainConfiguration {
  public static IServiceCollection AddDomain(this IServiceCollection services) {
    services.AddScoped<IMeasurementService,MeasurementService>();
    services.AddScoped<IUserService,UserService>();
    services.AddScoped<IAuthService,AuthService>();
    services.AddScoped<ISensorService,SensorService>();
    
    return services;
  }
}
