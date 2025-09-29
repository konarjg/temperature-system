using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Services.External;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TemperatureSystem.HostedServices;
using Xunit;

namespace UnitTests;

public class MeasurementSchedulerTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<ITemperatureSensorReader> _sensorReaderMock;
    private readonly Mock<IMeasurementService> _measurementServiceMock;
    private readonly Mock<ILogger<MeasurementScheduler>> _loggerMock;
    private readonly IConfiguration _configuration;

    public MeasurementSchedulerTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _sensorReaderMock = new Mock<ITemperatureSensorReader>();
        _measurementServiceMock = new Mock<IMeasurementService>();
        _loggerMock = new Mock<ILogger<MeasurementScheduler>>();

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Measurement:Interval", "1") }!)
            .Build();

        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITemperatureSensorReader))).Returns(_sensorReaderMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IMeasurementService))).Returns(_measurementServiceMock.Object);

        Mock<IServiceScope> scopeMock = new();
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        _scopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(scopeMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReadAndSaveMeasurements_WhenRunning()
    {
        // Arrange
        MeasurementScheduler scheduler = new(_scopeFactoryMock.Object, _configuration, _loggerMock.Object);
        List<Measurement> measurements = new()
        {
            new()
            {
                TemperatureCelsius = 22.0f,
                SensorId = 1,
                Timestamp = DateTime.UtcNow,
                Sensor = new Sensor { DisplayName = "Test Sensor", DeviceAddress = "test-addr" }
            }
        };
        _sensorReaderMock.Setup(r => r.ReadAsync()).ReturnsAsync(measurements);
        _measurementServiceMock.Setup(s => s.CreateRangeAsync(measurements)).ReturnsAsync(true);

        CancellationTokenSource cancellationTokenSource = new();

        // Act
        Task executeTask = scheduler.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(1500); // Allow the scheduler to run at least once
        cancellationTokenSource.Cancel();
        await executeTask;

        // Assert
        _sensorReaderMock.Verify(r => r.ReadAsync(), Times.AtLeastOnce);
        _measurementServiceMock.Verify(s => s.CreateRangeAsync(measurements), Times.AtLeastOnce);
    }
}