using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost; // <-- This 'using' is essential!
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using DatabaseAdapters;
using DatabaseAdapters.Repositories.SqLite;
using Domain.Services.External;
using ExternalServiceAdapters.EmailService;
using ExternalServiceAdapters.TemperatureSensorReader;

namespace IntegrationTests {
  /// <summary>
  /// A custom WebApplicationFactory for bootstrapping the application in-memory for integration tests.
  /// This factory is configured to:
  /// 1. Set AllowSynchronousIO = true on the TestServer.
  /// 2. Replace the production SqLiteDatabaseContext with an in-memory database.
  /// 3. Replace production external services with test doubles.
  /// </summary>
  public class CustomWebApplicationFactory : WebApplicationFactory<Program> {
    protected override void ConfigureWebHost(IWebHostBuilder builder) {
      builder.UseEnvironment("Testing");

      // This is the correct place for server-level configurations
      builder.ConfigureServer(options => {
        // This is the correct syntax to set AllowSynchronousIO
        options.AllowSynchronousIO = true;
      });

      builder.ConfigureServices(services => {
        // Find and remove the registration for the production database context.
        ServiceDescriptor? dbContextDescriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<SqLiteDatabaseContext>));

        if (dbContextDescriptor != null) {
          services.Remove(dbContextDescriptor);
        }

        // Remove the IDatabaseContext registration if it exists
         ServiceDescriptor? iDbContextDescriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(IDatabaseContext));

        if (iDbContextDescriptor != null) {
          services.Remove(iDbContextDescriptor);
        }

        // Add a new in-memory database context for testing.
        services.AddDbContext<IDatabaseContext, DatabaseAdapters.Repositories.Test.TestDatabaseContext>(options => {
          options.UseInMemoryDatabase("InMemoryDbForIntegrationTesting");
        });

        // Replace IEmailService with MockEmailService
        ServiceDescriptor? emailServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
        if(emailServiceDescriptor != null)
            services.Remove(emailServiceDescriptor);
        services.AddSingleton<IEmailService, MockEmailService>();

        // Replace ITemperatureSensorReader with FakeTemperatureSensorReader
        ServiceDescriptor? sensorReaderDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITemperatureSensorReader));
        if(sensorReaderDescriptor != null)
            services.Remove(sensorReaderDescriptor);
        services.AddSingleton<ITemperatureSensorReader, FakeTemperatureSensorReader>();
      });
    }
  }
}