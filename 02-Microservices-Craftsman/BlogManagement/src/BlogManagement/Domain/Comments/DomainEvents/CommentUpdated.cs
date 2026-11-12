namespace BlogManagement.Domain.Comments.DomainEvents;

public sealed class CommentUpdated : DomainEvent
{
    public Guid Id { get; set; } 
}
            