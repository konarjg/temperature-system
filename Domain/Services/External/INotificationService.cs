namespace Domain.Services.External;

public interface INotificationService<in T> {
  Task NotifyChangeAsync(T content);
}
