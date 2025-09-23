namespace TemperatureSystem.HostedServices;

using Domain.Entities;
using Domain.Services.External;
using Domain.Services.Interfaces;

public class MeasurementScheduler(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<MeasurementScheduler> logger) : BackgroundService {


  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    while (!stoppingToken.IsCancellationRequested) {
      using (IServiceScope scope = scopeFactory.CreateScope()) {
        ITemperatureSensorReader reader = scope.ServiceProvider.GetRequiredService<ITemperatureSensorReader>();
        IMeasurementService measurementService = scope.ServiceProvider.GetRequiredService<IMeasurementService>();

        bool success = await ReadTemperatureAsync(reader, measurementService);
        logger.LogInformation(success ? "Successfully read and saved the temperature measurement." : "Failed to read and save the temperature measurement.");
      }
      
      await Task.Delay(TimeSpan.FromSeconds(int.Parse(configuration["Measurement:Interval"] ?? "10")), stoppingToken);
    }
  }

  private async Task<bool> ReadTemperatureAsync(ITemperatureSensorReader reader, IMeasurementService measurementService) {
    List<Measurement> measurements = await reader.ReadAsync();

    foreach (Measurement measurement in measurements) {
      logger.LogInformation($"Temperature read from sensor {measurement.Sensor.DisplayName}: {measurement.TemperatureCelsius} C");
    }
    
    return await measurementService.CreateRangeAsync(measurements);
  }
}
