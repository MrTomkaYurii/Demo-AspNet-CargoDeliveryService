namespace BlogManagement.Domain.Users.Dtos;

using BlogManagement.Resources;

public sealed class UserParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
