namespace CargoOrderingManager.Domain.Drivers;

using System;
using System.Collections.Generic;
using CargoOrderingManager.Domain;
using CargoOrderingManager.Domain.Orders;
using CargoOrderingManager.Domain.Deliveries;
using CargoOrderingManager.Domain.Drivers.Models;

public class Driver : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string DriverLicenseNumber { get; private set; }
    public string Phone { get; private set; }

    public IReadOnlyCollection<Order> Orders { get; } = new List<Order>();
    public IReadOnlyCollection<Delivery> Deliveries { get; } = new List<Delivery>();

    public static Driver Create(DriverForCreation driverForCreation)
    {
        var newDriver = new Driver();
        newDriver.FirstName = driverForCreation.FirstName;
        newDriver.LastName = driverForCreation.LastName;
        newDriver.DriverLicenseNumber = driverForCreation.DriverLicenseNumber;
        newDriver.Phone = driverForCreation.Phone;

        return newDriver;
    }

    public static Driver CreateWithId(Guid id, string firstName, string lastName, string driverLicenseNumber = null, string phone = null)
    {
        var newDriver = new Driver();
        newDriver.Id = id;
        newDriver.FirstName = firstName;
        newDriver.LastName = lastName;
        newDriver.DriverLicenseNumber = driverLicenseNumber;
        newDriver.Phone = phone;

        return newDriver;
    }

    public Driver Update(DriverForUpdate driverForUpdate)
    {
        FirstName = driverForUpdate.FirstName;
        LastName = driverForUpdate.LastName;
        DriverLicenseNumber = driverForUpdate.DriverLicenseNumber;
        Phone = driverForUpdate.Phone;

        return this;
    }

    protected Driver() { } // For EF + Mocking
}
