using Domain.Entities;
using ExternalServiceAdapters.NotificationService;
using ExternalServiceAdapters.NotificationService.Measurement;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.ExternalServiceAdapters {
  public class SignalRMeasurementNotificationServiceTests {
    private readonly Mock<IHubContext<MeasurementHub>> _hubContextMock;
    private readonly Mock<ILogger<SignalRMeasurementNotificationService>> _loggerMock;
    private readonly Mock<IHubClients> _hubClientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly SignalRMeasurementNotificationService _notificationService;

    public SignalRMeasurementNotificationServiceTests() {
      _hubContextMock = new Mock<IHubContext<MeasurementHub>>();
      _loggerMock = new Mock<ILogger<SignalRMeasurementNotificationService>>();
      _hubClientsMock = new Mock<IHubClients>();
      _clientProxyMock = new Mock<IClientProxy>();

      // Setup the mock chain for SignalR
      _hubContextMock.Setup(h => h.Clients).Returns(_hubClientsMock.Object);
      _hubClientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);

      _notificationService = new SignalRMeasurementNotificationService(_hubContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task NotifyChangeAsync_ShouldSendNotificationToCorrectGroup() {
      // Arrange
      object[]? capturedArgs = null;
      string? capturedMethod = null;

      Measurement measurement = new() {
        Id = 1,
        TemperatureCelsius = 22.5f,
        Timestamp = System.DateTime.UtcNow,
        SensorId = 101,
        Sensor = new Sensor { Id = 101, DisplayName = "Test Sensor", DeviceAddress = "test-addr", State = Domain.Entities.Util.SensorState.Operational }
      };

      // Setup the mock to capture the arguments passed to SendCoreAsync
      _clientProxyMock.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
        .Callback<string, object[], CancellationToken>((method, args, token) => {
          capturedMethod = method;
          capturedArgs = args;
        })
        .Returns(Task.CompletedTask);

      // Act
      await _notificationService.NotifyChangeAsync(measurement);

      // Assert
      // Verify that the service attempted to send to the correct group
      _hubClientsMock.Verify(c => c.Group("101"), Times.Once);

      // Verify the method name and payload that were captured
      Assert.Equal("ReceiveMeasurement", capturedMethod);
      Assert.NotNull(capturedArgs);
      Assert.Single(capturedArgs);
      Assert.IsAssignableFrom<MeasurementNotification>(capturedArgs[0]);

      MeasurementNotification dto = (MeasurementNotification)capturedArgs[0]!;
      Assert.Equal(measurement.TemperatureCelsius, dto.TemperatureCelsius);
      Assert.NotEmpty(dto.Timestamp);
    }
  }
}