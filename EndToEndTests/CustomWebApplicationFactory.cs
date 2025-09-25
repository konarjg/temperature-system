using System.Collections.Generic;
using System.Linq;
using DatabaseAdapters.Repositories.SqLite;
using Domain.Services.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EndToEndTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<SqLiteDatabaseContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            var sensorDefinitionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(List<SensorDefinition>));

            if (sensorDefinitionsDescriptor != null)
            {
                services.Remove(sensorDefinitionsDescriptor);
            }

            services.AddDbContext<SqLiteDatabaseContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            services.AddSingleton(new List<SensorDefinition>
            {
                new() { Address = "test-sensor-1", DisplayName = "Test Sensor 1" },
                new() { Address = "test-sensor-2", DisplayName = "Test Sensor 2" }
            });

            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<SqLiteDatabaseContext>();

            db.Database.EnsureCreated();
        });
    }
}