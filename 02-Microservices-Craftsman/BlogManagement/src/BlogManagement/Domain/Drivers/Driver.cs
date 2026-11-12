using BlogManagement.Domain.PostAboutDrivers;
using BlogManagement.Domain.PostAboutDrivers.Models;
using BlogManagement.Domain.Likes;
using BlogManagement.Domain.Drivers.Models;
using BlogManagement.Domain.Drivers.DomainEvents;

namespace BlogManagement.Domain.Drivers;
public class Driver : BaseEntity
{
    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public IReadOnlyCollection<Like> Likes { get; } = new List<Like>();

    // Без ініціалізатора: інакше кожен тіньовий водій створював би порожній пост у блозі.
    public PostAboutDriver PostAboutDriver { get; private set; }

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static Driver Create(DriverForCreation driverForCreation)
    {
        var newDriver = new Driver();

        newDriver.FirstName = driverForCreation.FirstName;
        newDriver.LastName = driverForCreation.LastName;

        newDriver.QueueDomainEvent(new DriverCreated(){ Driver = newDriver });
        
        return newDriver;
    }

    public static Driver CreateWithId(Guid id, string firstName, string lastName)
    {
        var newDriver = new Driver();
        newDriver.Id = id;
        newDriver.FirstName = firstName;
        newDriver.LastName = lastName;
        return newDriver;
    }

    public Driver Update(DriverForUpdate driverForUpdate)
    {
        FirstName = driverForUpdate.FirstName;
        LastName = driverForUpdate.LastName;

        QueueDomainEvent(new DriverUpdated(){ Id = Id });
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected Driver() { } // For EF + Mocking
}
