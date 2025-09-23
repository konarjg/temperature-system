namespace DatabaseAdapters.Repositories.Test;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class TestDatabaseConfiguration {
  public static IServiceCollection AddTestDatabaseContext(this IServiceCollection services, IConfiguration configuration) {
    services.AddDbContext<TestDatabaseContext>(options => options.UseInMemoryDatabase(configuration.GetConnectionString("Test") ?? "test"));
    services.AddScoped<IDatabaseContext, TestDatabaseContext>();

    return services;
  }
}
