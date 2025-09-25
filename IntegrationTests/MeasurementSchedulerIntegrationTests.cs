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
    private readonly IServiceScope _scope;
    private readonly TestDatabaseContext _dbContext;
    private readonly Mock<ITemperatureSensorReader> _sensorReaderMock;
    private readonly IServiceProvider _serviceProvider;

    public MeasurementSchedulerIntegrationTests()
    {
        var services = new ServiceCollection();
        _sensorReaderMock = new Mock<ITemperatureSensorReader>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Measurement:Interval", "1") })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddDbContext<TestDatabaseContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<TestDatabaseContext>());
        services.AddScoped<IDatabaseContext>(provider => provider.GetRequiredService<TestDatabaseContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IMeasurementRepository, MeasurementRepository>();
        services.AddScoped<IMeasurementService, MeasurementService>();
        services.AddSingleton(_sensorReaderMock.Object);
        services.AddSingleton(new Mock<ILogger<MeasurementScheduler>>().Object);

        services.AddHostedService<MeasurementScheduler>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<TestDatabaseContext>();
    }

    [Fact]
    public async Task MeasurementScheduler_ShouldReadFromSensorAndSaveToDatabase()
    {
        // Arrange
        var scheduler = _serviceProvider.GetServices<IHostedService>().OfType<MeasurementScheduler>().Single();
        var measurementsToRead = new List<Measurement>
        {
            new() { TemperatureCelsius = 25.0f, SensorId = 1, Sensor = new Sensor() },
            new() { TemperatureCelsius = 35.0f, SensorId = 2, Sensor = new Sensor() }
        };
        _sensorReaderMock.Setup(r => r.ReadAsync()).ReturnsAsync(measurementsToRead);

        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await scheduler.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(1500, cancellationTokenSource.Token); // Wait for the scheduler to run at least once
        cancellationTokenSource.Cancel();

        // Assert
        var savedMeasurements = await _dbContext.Measurements.AsNoTracking().ToListAsync();
        Assert.Equal(2, savedMeasurements.Count);
        Assert.Contains(savedMeasurements, m => m.TemperatureCelsius == 25.0f);
        Assert.Contains(savedMeasurements, m => m.TemperatureCelsius == 35.0f);
        _sensorReaderMock.Verify(r => r.ReadAsync(), Times.AtLeastOnce);
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}