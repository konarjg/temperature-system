using System.Collections.Generic;
using System.Linq;
using DatabaseAdapters;
using DatabaseAdapters.Repositories;
using DatabaseAdapters.Repositories.SqLite;
using Domain.Repositories;
using Domain.Services.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EndToEndTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseTestServer(options => options.AllowSynchronousIO = true);

        builder.ConfigureServices(services =>
        {
            // Remove all registrations related to the real database
            var dbRelatedDescriptors = services.Where(d =>
                d.ServiceType.FullName?.Contains("DatabaseAdapters") == true ||
                d.ImplementationType?.FullName?.Contains("DatabaseAdapters") == true).ToList();

            foreach (var descriptor in dbRelatedDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove sensor definitions from appsettings.json
            var sensorDefinitionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(List<SensorDefinition>));
            if (sensorDefinitionsDescriptor != null)
            {
                services.Remove(sensorDefinitionsDescriptor);
            }

            // Remove SensorSync hosted service to prevent it from running during tests
            var sensorSyncDescriptor = services.SingleOrDefault(d => d.ImplementationType == typeof(TemperatureSystem.HostedServices.SensorSync));
            if (sensorSyncDescriptor != null)
            {
                services.Remove(sensorSyncDescriptor);
            }

            // Add test database
            services.AddDbContext<SqLiteDatabaseContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });
            services.AddScoped<IDatabaseContext, SqLiteDatabaseContext>();

            // Re-add repositories with the test database
            services.AddScoped<IMeasurementRepository, MeasurementRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();
            services.AddScoped<ISensorRepository, SensorRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add test sensor definitions
            services.AddSingleton(new List<SensorDefinition>
            {
                new("Test Sensor 1", "test-sensor-1"),
                new("Test Sensor 2", "test-sensor-2")
            });

            // Ensure the test database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SqLiteDatabaseContext>();
            db.Database.EnsureCreated();
        });
    }
}