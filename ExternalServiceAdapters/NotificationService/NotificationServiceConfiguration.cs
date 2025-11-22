namespace ExternalServiceAdapters.NotificationService;

using Domain.Services.External;
using Measurement;
using Microsoft.Extensions.DependencyInjection;

public static class NotificationServiceConfiguration {
  public static IServiceCollection AddNotificationServices(this IServiceCollection services) {
    services.AddSignalR();
    services.AddSingleton<INotificationService<List<Domain.Entities.Measurement>>, SignalRMeasurementNotificationService>();

    return services;
  }
}
