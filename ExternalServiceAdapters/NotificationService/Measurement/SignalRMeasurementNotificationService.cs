namespace ExternalServiceAdapters.NotificationService.Measurement;

using Domain.Entities;
using Domain.Services.External;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SignalRMeasurementNotificationService(
    IHubContext<MeasurementHub> hubContext, 
    ILogger<SignalRMeasurementNotificationService> logger) : INotificationService<Measurement> {

    private const string ClientMethodName = "ReceiveMeasurement";
  
    public async Task NotifyChangeAsync(Measurement measurement) {
        logger.LogInformation($"Broadcasting measurement {measurement.TemperatureCelsius} *C at {measurement.Timestamp}");
        
        string groupId = measurement.SensorId.ToString();

        await hubContext.Clients
                        .Group(groupId)
                        .SendAsync(ClientMethodName,measurement.ToNotification());
    }
}