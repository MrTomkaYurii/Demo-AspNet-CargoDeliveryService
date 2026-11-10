namespace CargoOrderingManager.Domain.Deliveries.Services;

using CargoOrderingManager.Domain.Deliveries;
using CargoOrderingManager.Databases;
using CargoOrderingManager.Services;

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
