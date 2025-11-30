namespace ExternalServiceAdapters.NotificationService.Sensor;

using Domain.Entities;
using Domain.Services.External;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

public class SignalRSensorNotificationService(IHubContext<SensorHub> hubContext, ILogger<SignalRSensorNotificationService> logger) : INotificationService<Sensor> {

  private const string ClientMethodName = "UpdateSensor";
  
  public async Task NotifyChangeAsync(Sensor sensor) {
    logger.LogInformation($"Broadcasting sensor {sensor.DisplayName} state change to {sensor.State}");

    string groupId = nameof(Sensor);

    await hubContext.Clients
                    .Group(groupId)
                    .SendAsync(ClientMethodName,sensor.ToNotification());
  }
}
