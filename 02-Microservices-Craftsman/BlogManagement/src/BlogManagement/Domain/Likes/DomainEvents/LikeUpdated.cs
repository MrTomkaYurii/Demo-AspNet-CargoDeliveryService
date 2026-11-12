namespace BlogManagement.Domain.Likes.DomainEvents;

public sealed class LikeUpdated : DomainEvent
{
    public Guid Id { get; set; } 
}
            