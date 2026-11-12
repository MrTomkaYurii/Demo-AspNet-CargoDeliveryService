namespace BlogManagement.Domain.Likes.Services;

using BlogManagement.Domain.Likes;
using BlogManagement.Databases;
using BlogManagement.Services;

public interface ILikeRepository : IGenericRepository<Like>
{
}

public sealed class LikeRepository : GenericRepository<Like>, ILikeRepository
{
    private readonly BlogManagementContext _dbContext;

    public LikeRepository(BlogManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}
