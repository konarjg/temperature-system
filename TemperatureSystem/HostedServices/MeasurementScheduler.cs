namespace TemperatureSystem.HostedServices;

using Domain.Entities;
using Domain.Services.External;
using Domain.Services.Interfaces;

public class MeasurementScheduler(IServiceScopeFactory scopeFactory, INotificationService<Measurement> notificationService, IConfiguration configuration, ILogger<MeasurementScheduler> logger) : BackgroundService {
  
  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    while (!stoppingToken.IsCancellationRequested) {
      using IServiceScope scope = scopeFactory.CreateScope();
      
      IMeasurementService measurementService = scope.ServiceProvider.GetRequiredService<IMeasurementService>();

      bool success = await measurementService.PerformMeasurements();
      logger.LogInformation(success ? "Successfully read and saved the temperature measurement." : "Failed to read and save the temperature measurement.");

      await Task.Delay(TimeSpan.FromSeconds(int.Parse(configuration["Measurement:Interval"] ?? "10")), stoppingToken);
    }
  }
}
