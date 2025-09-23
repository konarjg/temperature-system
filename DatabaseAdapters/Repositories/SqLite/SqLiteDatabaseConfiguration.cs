namespace DatabaseAdapters.Repositories.SqLite;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class SqLiteDatabaseContextConfiguration {
  public static IServiceCollection AddSqLiteDatabaseContext(this IServiceCollection services, IConfiguration configuration) {
    services.AddDbContext<SqLiteDatabaseContext>(options => options.UseSqlite(configuration.GetConnectionString(configuration["Environment"] ?? "DefaultConnection")));
    services.AddScoped<IDatabaseContext, SqLiteDatabaseContext>();

    return services;
  }
}
