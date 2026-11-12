namespace OrderingManagement.Domain.Orders.Services;

using OrderingManagement.Domain.Orders;
using OrderingManagement.Databases;
using OrderingManagement.Services;

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
