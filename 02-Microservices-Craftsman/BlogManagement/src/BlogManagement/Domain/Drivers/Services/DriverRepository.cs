namespace BlogManagement.Domain.Drivers.Services;

using BlogManagement.Domain.Drivers;
using BlogManagement.Databases;
using BlogManagement.Services;
using BlogManagement.Domain.Drivers.Dtos;
using BlogManagement.Domain.PostAboutDrivers.Dtos;
using Microsoft.EntityFrameworkCore;

public interface IDriverRepository : IGenericRepository<Driver>
{
    Task<DriverWithPostsDto> GetDriverWithPosts(Guid driverId);
}

public sealed class DriverRepository : GenericRepository<Driver>, IDriverRepository
{
    private readonly BlogManagementContext _dbContext;

    public DriverRepository(BlogManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DriverWithPostsDto> GetDriverWithPosts(Guid driverId)
    {
        var driverWithPosts = await _dbContext.Drivers
            .Where(driver => driver.Id == driverId)
            .Select(driver => new DriverWithPostsDto
            {
                DriverId = driver.Id,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
                Posts = _dbContext.PostAboutDrivers
                    .Where(post => post.Driver.Id == driverId)
                    .Select(post => new PostAboutDriverDto
                    {
                        Id = post.Id,
                        Title = post.Title,
                        Content = post.Content
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();

        return driverWithPosts;
    }

}
