namespace BlogManagement.Domain.Likes.Dtos;

using BlogManagement.Resources;

public sealed class LikeParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
