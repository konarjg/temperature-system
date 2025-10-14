using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Linq;
using DatabaseAdapters;
using DatabaseAdapters.Repositories.SqLite;
using Domain.Services.External;
using ExternalServiceAdapters.EmailService;
using ExternalServiceAdapters.TemperatureSensorReader;

namespace IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<SqLiteDatabaseContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                var iDbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDatabaseContext));
                if (iDbContextDescriptor != null)
                {
                    services.Remove(iDbContextDescriptor);
                }

                services.AddDbContext<IDatabaseContext, DatabaseAdapters.Repositories.Test.TestDatabaseContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForIntegrationTesting");
                });

                var emailServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
                if (emailServiceDescriptor != null)
                    services.Remove(emailServiceDescriptor);
                services.AddSingleton<IEmailService, MockEmailService>();

                var sensorReaderDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITemperatureSensorReader));
                if (sensorReaderDescriptor != null)
                    services.Remove(sensorReaderDescriptor);
                services.AddSingleton<ITemperatureSensorReader, FakeTemperatureSensorReader>();
            });
        }
    }
}