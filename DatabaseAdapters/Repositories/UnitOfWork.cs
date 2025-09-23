namespace DatabaseAdapters.Repositories;

using Domain.Repositories;

public class UnitOfWork(IDatabaseContext databaseContext) : IUnitOfWork {

  public async Task<int> CompleteAsync() {
    return await databaseContext.SaveChangesAsync();
  }
}
