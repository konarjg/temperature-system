namespace Domain.Repositories;

public interface IUnitOfWork {
  Task<long> CompleteAsync();
}
