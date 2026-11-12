using BlogManagement.Domain.PostAboutDrivers.Dtos;

namespace BlogManagement.Domain.Drivers.Dtos
{
    public sealed class DriverWithPostsDto
    {
        public Guid DriverId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<PostAboutDriverDto> Posts { get; init; }
    }
}
