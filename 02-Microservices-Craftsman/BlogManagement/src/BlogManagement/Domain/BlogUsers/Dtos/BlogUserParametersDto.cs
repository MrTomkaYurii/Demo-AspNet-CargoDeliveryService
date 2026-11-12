namespace BlogManagement.Domain.BlogUsers.Dtos;

using BlogManagement.Resources;

public sealed class BlogUserParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
