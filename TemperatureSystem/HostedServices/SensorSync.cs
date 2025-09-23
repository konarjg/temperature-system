namespace TemperatureSystem.HostedServices;

using Domain.Services.Interfaces;
using Domain.Services.Util;

public class SensorSync(IServiceScopeFactory serviceScopeFactory, List<SensorDefinition> sensorDefinitions) : IHostedService{

  public async Task StartAsync(CancellationToken cancellationToken) {
    using (IServiceScope scope = serviceScopeFactory.CreateScope()) {
      ISensorService sensorService = scope.ServiceProvider.GetRequiredService<ISensorService>();
      await sensorService.SyncSensorsAsync(sensorDefinitions);
    }
  }

  public Task StopAsync(CancellationToken cancellationToken) {
    return Task.CompletedTask;
  }
}
