namespace BlogManagement.Services;

using BlogManagement.Databases;

public interface IUnitOfWork : IBlogManagementScopedService
{
    Task<int> CommitChanges(CancellationToken cancellationToken = default);
}

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BlogManagementContext _dbContext;

    public UnitOfWork(BlogManagementContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CommitChanges(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
