namespace ExternalServiceAdapters.NotificationService.Measurement;

using Microsoft.AspNetCore.SignalR;

public class MeasurementHub : Hub {
  public async Task SubscribeToSensor(long sensorId) {
    await Groups.AddToGroupAsync(Context.ConnectionId, sensorId.ToString());
  }
  
  public async Task UnsubscribeFromSensor(long sensorId) {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, sensorId.ToString());
  }
}
