namespace CargoOrderingManager.Domain.Orders.DomainEvents;

public sealed class OrderCreated : DomainEvent
{
    public Order Order { get; set; } 
}
            