namespace ExternalServiceAdapters.NotificationService;

using Domain.Services.External;
using Measurement;
using Microsoft.Extensions.DependencyInjection;
using Sensor;

public static class NotificationServiceConfiguration {
  public static IServiceCollection AddNotificationServices(this IServiceCollection services) {
    services.AddSignalR();
    services.AddSingleton<INotificationService<Domain.Entities.Measurement>, SignalRMeasurementNotificationService>();
    services.AddSingleton<INotificationService<Domain.Entities.Sensor>,SignalRSensorNotificationService>();

    return services;
  }
}
