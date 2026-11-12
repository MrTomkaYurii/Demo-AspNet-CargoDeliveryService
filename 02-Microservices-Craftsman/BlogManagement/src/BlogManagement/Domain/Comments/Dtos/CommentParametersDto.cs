namespace BlogManagement.Domain.Comments.Dtos;

using BlogManagement.Resources;

public sealed class CommentParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
