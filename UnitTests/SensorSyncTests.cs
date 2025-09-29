using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Services.Interfaces;
using Domain.Services.Util;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TemperatureSystem.HostedServices;
using Xunit;

namespace UnitTests;

public class SensorSyncTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<ISensorService> _sensorServiceMock;
    private readonly List<SensorDefinition> _sensorDefinitions;

    public SensorSyncTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _sensorServiceMock = new Mock<ISensorService>();
        _sensorDefinitions = new List<SensorDefinition>
        {
            new("Sensor 1", "addr1")
        };

        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ISensorService)))
            .Returns(_sensorServiceMock.Object);

        Mock<IServiceScope> scopeMock = new();
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        _scopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(scopeMock.Object);
    }

    [Fact]
    public async Task StartAsync_ShouldCallSyncSensorsAsyncWithDefinitions()
    {
        // Arrange
        SensorSync sensorSync = new(_scopeFactoryMock.Object, _sensorDefinitions);
        CancellationToken cancellationToken = new();

        // Act
        await sensorSync.StartAsync(cancellationToken);

        // Assert
        _sensorServiceMock.Verify(s => s.SyncSensorsAsync(_sensorDefinitions), Times.Once);
    }
}