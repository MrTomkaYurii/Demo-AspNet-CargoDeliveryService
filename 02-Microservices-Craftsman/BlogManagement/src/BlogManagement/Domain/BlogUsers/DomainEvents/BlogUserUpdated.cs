namespace BlogManagement.Domain.BlogUsers.DomainEvents;

public sealed class BlogUserUpdated : DomainEvent
{
    public Guid Id { get; set; } 
}
            