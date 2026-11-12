namespace BlogManagement.Domain.RolePermissions.Dtos;

using BlogManagement.Resources;

public sealed class RolePermissionParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
