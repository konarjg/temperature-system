namespace ExternalServiceAdapters.NotificationService.Sensor;

using Microsoft.AspNetCore.SignalR;

public class SensorHub : Hub {
  
  public async Task Subscribe() {
    await Groups.AddToGroupAsync(Context.ConnectionId, nameof(Sensor));
  }
  
  public async Task Unsubscribe() {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, nameof(Sensor));
  }
}
