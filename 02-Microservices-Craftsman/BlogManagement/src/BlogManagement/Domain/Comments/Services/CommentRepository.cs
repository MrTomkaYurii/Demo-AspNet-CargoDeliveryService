namespace BlogManagement.Domain.Comments.Services;

using BlogManagement.Domain.Comments;
using BlogManagement.Databases;
using BlogManagement.Services;

public interface ICommentRepository : IGenericRepository<Comment>
{
}

public sealed class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    private readonly BlogManagementContext _dbContext;

    public CommentRepository(BlogManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}
