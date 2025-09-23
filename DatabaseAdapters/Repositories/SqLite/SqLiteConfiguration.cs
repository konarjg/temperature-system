namespace DatabaseAdapters.Repositories.SqLite;

using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class SqLiteConfiguration {
  public static IServiceCollection AddSqLiteDatabaseAdapter(this IServiceCollection services,
    IConfiguration configuration) {

    services.AddSqLiteDatabaseContext(configuration);
    services.AddScoped<IMeasurementRepository,MeasurementRepository>();
    services.AddScoped<IUserRepository,UserRepository>();
    services.AddScoped<IRefreshTokenRepository,RefreshTokenRepository>();
    services.AddScoped<IVerificationTokenRepository,VerificationTokenRepository>();
    services.AddScoped<ISensorRepository,SensorRepository>();
    services.AddScoped<IUnitOfWork,UnitOfWork>();

    return services;
  }
}
