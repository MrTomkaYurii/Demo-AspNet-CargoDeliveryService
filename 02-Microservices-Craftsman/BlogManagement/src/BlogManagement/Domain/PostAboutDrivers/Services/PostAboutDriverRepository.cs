namespace BlogManagement.Domain.PostAboutDrivers.Services;

using BlogManagement.Domain.PostAboutDrivers;
using BlogManagement.Databases;
using BlogManagement.Services;

public interface IPostAboutDriverRepository : IGenericRepository<PostAboutDriver>
{
}

public sealed class PostAboutDriverRepository : GenericRepository<PostAboutDriver>, IPostAboutDriverRepository
{
    private readonly BlogManagementContext _dbContext;

    public PostAboutDriverRepository(BlogManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}
