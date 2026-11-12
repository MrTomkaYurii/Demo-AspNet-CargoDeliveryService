namespace BlogManagement.Domain.Drivers.Dtos;

using BlogManagement.Resources;

public sealed class DriverParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
