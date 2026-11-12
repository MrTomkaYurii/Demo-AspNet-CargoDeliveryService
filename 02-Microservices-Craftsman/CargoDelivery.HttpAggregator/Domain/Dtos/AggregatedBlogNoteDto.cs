namespace CargoDelivery.HttpAggregator.Domain.Dtos;

/// <summary>
/// Агреговані дані нотатки / поста з мікросервісу BlogManagement.
/// </summary>
public sealed class AggregatedBlogNoteDto
{
    public Guid PostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
