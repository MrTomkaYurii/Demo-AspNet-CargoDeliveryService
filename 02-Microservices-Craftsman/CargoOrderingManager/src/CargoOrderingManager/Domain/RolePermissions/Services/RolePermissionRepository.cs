namespace CargoOrderingManager.Domain.RolePermissions.Services;

using CargoOrderingManager.Domain.RolePermissions;
using CargoOrderingManager.Databases;
using CargoOrderingManager.Services;

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
