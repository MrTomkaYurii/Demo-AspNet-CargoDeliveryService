namespace BlogManagement.Domain.Comments.DomainEvents;

public sealed class CommentCreated : DomainEvent
{
    public Comment Comment { get; set; } 
}
            