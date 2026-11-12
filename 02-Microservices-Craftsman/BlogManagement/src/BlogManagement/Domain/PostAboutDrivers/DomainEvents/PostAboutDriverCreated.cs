namespace BlogManagement.Domain.PostAboutDrivers.DomainEvents;

public sealed class PostAboutDriverCreated : DomainEvent
{
    public PostAboutDriver PostAboutDriver { get; set; } 
}
            