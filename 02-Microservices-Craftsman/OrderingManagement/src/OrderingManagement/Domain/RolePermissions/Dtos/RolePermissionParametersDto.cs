namespace OrderingManagement.Domain.RolePermissions.Dtos;

using OrderingManagement.Resources;

public sealed class RolePermissionParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
