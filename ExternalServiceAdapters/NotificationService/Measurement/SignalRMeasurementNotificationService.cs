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
    ILogger<SignalRMeasurementNotificationService> logger) : INotificationService<List<Measurement>> {

    private const string ClientMethodName = "ReceiveMeasurement";
  
    public async Task NotifyChangeAsync(List<Measurement> contents) {
        if (contents.Count == 0) {
            return;
        }
        
        logger.LogInformation(
            "Broadcasting {Count} measurements to SignalR clients. Method: {ClientMethod}", 
            contents.Count, 
            ClientMethodName
        );
        
        List<Task> notifications = contents.Select(content => {
            string groupId = content.SensorId.ToString();

            return hubContext.Clients
                .Group(groupId)
                .SendAsync(ClientMethodName, content.ToNotification());
        }).ToList();
        
        await Task.WhenAll(notifications);
    }
}