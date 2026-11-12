namespace OrderingManagement.Domain.RolePermissions.Services;

using OrderingManagement.Domain.RolePermissions;
using OrderingManagement.Databases;
using OrderingManagement.Services;

public interface IRolePermissionRepository : IGenericRepository<RolePermission>
{
}

public sealed class RolePermissionRepository : GenericRepository<RolePermission>, IRolePermissionRepository
{
    private readonly OrderingContext _dbContext;

    public RolePermissionRepository(OrderingContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}
