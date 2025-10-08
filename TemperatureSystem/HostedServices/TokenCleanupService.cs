namespace TemperatureSystem.HostedServices;

using Domain.Entities;
using Domain.Repositories;

public class TokenCleanupService(ILogger<TokenCleanupService> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration) : BackgroundService {

  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    while (!stoppingToken.IsCancellationRequested) {
      try {
        using IServiceScope scope = scopeFactory.CreateAsyncScope();
        
        IRefreshTokenRepository refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
        IVerificationTokenRepository verificationTokenRepository = scope.ServiceProvider.GetRequiredService<IVerificationTokenRepository>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await CleanupRefreshTokens(refreshTokenRepository);
        await CleanupVerificationTokens(verificationTokenRepository);
        logger.LogInformation($"Removed {await unitOfWork.CompleteAsync()} inactive tokens.");
      } catch (Exception ex) {
        logger.LogError(ex, "An error occurred while cleaning up tokens.");
      }
      
      await Task.Delay(TimeSpan.FromDays(configuration.GetValue("Jwt:TokenCleanupDelayDays", 1)),stoppingToken);
    }
  }

  private async Task CleanupRefreshTokens(IRefreshTokenRepository refreshTokenRepository) {
    foreach (RefreshToken token in await refreshTokenRepository.GetAllInactiveAsync()){
      refreshTokenRepository.Remove(token);
    }
  }

  private async Task CleanupVerificationTokens(IVerificationTokenRepository verificationTokenRepository) {
    foreach (VerificationToken token in await verificationTokenRepository.GetAllInactiveAsync()) {
      verificationTokenRepository.Remove(token);
    }
  }
}
