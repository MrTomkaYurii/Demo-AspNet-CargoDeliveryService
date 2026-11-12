namespace BlogManagement.Domain.PostAboutDrivers.Dtos;

using BlogManagement.Resources;

public sealed class PostAboutDriverParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
