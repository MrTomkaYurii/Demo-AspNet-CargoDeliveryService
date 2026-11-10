namespace CargoOrderingManager.Domain.RolePermissions.Dtos;

using CargoOrderingManager.Resources;

public sealed class RolePermissionParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
