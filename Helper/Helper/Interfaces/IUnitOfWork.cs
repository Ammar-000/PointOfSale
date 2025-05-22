namespace Helper.Interfaces;

public interface IUnitOfWork
{
    public Task BeginTransactionAsync();
    public Task CommitAsync();
    public Task RollbackAsync();
}
