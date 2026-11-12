namespace OrderingManagement.Domain.Deliveries.Services;

using OrderingManagement.Domain.Deliveries;
using OrderingManagement.Databases;
using OrderingManagement.Services;

public interface IDeliveryRepository : IGenericRepository<Delivery>
{
}

public sealed class DeliveryRepository : GenericRepository<Delivery>, IDeliveryRepository
{
    private readonly OrderingContext _dbContext;

    public DeliveryRepository(OrderingContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}
