namespace BlogManagement.Domain.RolePermissions.Services;

using BlogManagement.Domain.RolePermissions;
using BlogManagement.Databases;
using BlogManagement.Services;

public interface IRolePermissionRepository : IGenericRepository<RolePermission>
{
}

public sealed class RolePermissionRepository : GenericRepository<RolePermission>, IRolePermissionRepository
{
    private readonly BlogManagementContext _dbContext;

    public RolePermissionRepository(BlogManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}
