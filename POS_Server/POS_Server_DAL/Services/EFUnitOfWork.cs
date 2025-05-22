using Helper.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using POS_Server_DAL.Repositories.Implementations.EFRepositories.Context;

namespace POS_Server_DAL.Services;

public class EFUnitOfWork : IUnitOfWork
{
    readonly EFDbContext _dbContext;
    IDbContextTransaction _transaction;

    public EFUnitOfWork(EFDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        await _transaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        await _transaction.RollbackAsync();
    }

}
