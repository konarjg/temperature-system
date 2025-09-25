using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DatabaseAdapters;
using DatabaseAdapters.Repositories;
using DatabaseAdapters.Repositories.Test;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.External;
using Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using TemperatureSystem.HostedServices;
using Xunit;

namespace IntegrationTests;

public class MeasurementSchedulerIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<ITemperatureSensorReader> _sensorReaderMock;

    public MeasurementSchedulerIntegrationTests()
    {
        _sensorReaderMock = new Mock<ITemperatureSensorReader>();
        string dbName = Guid.NewGuid().ToString();
        ServiceCollection services = new();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Measurement:Interval", "1") }!)
            .Build();

        services.AddSingleton(configuration);

        services.AddDbContext<TestDatabaseContext>(options =>
            options.UseInMemoryDatabase(databaseName: dbName));
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<TestDatabaseContext>());
        services.AddScoped<IDatabaseContext>(provider => provider.GetRequiredService<TestDatabaseContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IMeasurementRepository, MeasurementRepository>();
        services.AddScoped<IMeasurementService, MeasurementService>();
        services.AddSingleton(_sensorReaderMock.Object);
        services.AddSingleton(new Mock<ILogger<MeasurementScheduler>>().Object);

        services.AddHostedService<MeasurementScheduler>();

        _serviceProvider = services.BuildServiceProvider();

        // Seed the database
        using IServiceScope scope = _serviceProvider.CreateScope();
        TestDatabaseContext dbContext = scope.ServiceProvider.GetRequiredService<TestDatabaseContext>();
        dbContext.Sensors.Add(new Sensor { Id = 1, DisplayName = "Test Sensor 1", DeviceAddress = "test-addr-1" });
        dbContext.Sensors.Add(new Sensor { Id = 2, DisplayName = "Test Sensor 2", DeviceAddress = "test-addr-2" });
        dbContext.SaveChanges();
    }

    [Fact]
    public async Task MeasurementScheduler_ShouldReadFromSensorAndSaveToDatabase()
    {
        // Arrange
        IHostedService scheduler = _serviceProvider.GetServices<IHostedService>().OfType<MeasurementScheduler>().Single();
        List<Measurement> measurementsToRead = new()
        {
            new() { TemperatureCelsius = 25.0f, SensorId = 1, Timestamp = DateTime.UtcNow },
            new() { TemperatureCelsius = 35.0f, SensorId = 2, Timestamp = DateTime.UtcNow }
        };
        _sensorReaderMock.Setup(r => r.ReadAsync()).ReturnsAsync(measurementsToRead);

        // Act
        await scheduler.StartAsync(CancellationToken.None);
        await Task.Delay(1500); // Wait for scheduler to run once (interval is 1s)
        await scheduler.StopAsync(CancellationToken.None);

        // Assert
        using IServiceScope assertScope = _serviceProvider.CreateScope();
        TestDatabaseContext dbContext = assertScope.ServiceProvider.GetRequiredService<TestDatabaseContext>();
        List<Measurement> savedMeasurements = await dbContext.Measurements.AsNoTracking().ToListAsync();

        Assert.Equal(2, savedMeasurements.Count);
        Assert.Contains(savedMeasurements, m => m.TemperatureCelsius == 25.0f);
        Assert.Contains(savedMeasurements, m => m.TemperatureCelsius == 35.0f);
        _sensorReaderMock.Verify(r => r.ReadAsync(), Times.AtLeastOnce);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}