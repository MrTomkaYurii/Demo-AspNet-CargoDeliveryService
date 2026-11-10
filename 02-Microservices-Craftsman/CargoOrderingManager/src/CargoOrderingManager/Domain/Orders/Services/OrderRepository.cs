namespace CargoOrderingManager.Domain.Orders.Services;

using CargoOrderingManager.Domain.Orders;
using CargoOrderingManager.Databases;
using CargoOrderingManager.Services;

public interface IOrderRepository : IGenericRepository<Order>
{
}

public sealed class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    private readonly OrderingContext _dbContext;

    public OrderRepository(OrderingContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}
