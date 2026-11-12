namespace BlogManagement.Domain.BlogUsers.Services;

using BlogManagement.Domain.BlogUsers;
using BlogManagement.Databases;
using BlogManagement.Services;

public interface IBlogUserRepository : IGenericRepository<BlogUser>
{
}

public sealed class BlogUserRepository : GenericRepository<BlogUser>, IBlogUserRepository
{
    private readonly BlogManagementContext _dbContext;

    public BlogUserRepository(BlogManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}
