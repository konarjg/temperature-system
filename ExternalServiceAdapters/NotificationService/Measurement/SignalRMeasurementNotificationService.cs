namespace ExternalServiceAdapters.NotificationService.Measurement;

using Domain.Entities;
using Domain.Services.External;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

public class SignalRMeasurementNotificationService(IHubContext<MeasurementHub> hubContext, ILogger<SignalRMeasurementNotificationService> logger) : INotificationService<Measurement> {

  private const string ClientMethodName = "ReceiveMeasurement";
  
  public async Task NotifyChangeAsync(Measurement content) {
    string groupId = content.SensorId.ToString();

    logger.LogInformation(
      "Sending real-time notification for SensorId {SensorId}. Client method: {ClientMethod}", 
      groupId, 
      ClientMethodName
    );
    
    await hubContext.Clients
                     .Group(groupId)
                     .SendAsync(ClientMethodName, content.ToNotification());
  }
}
