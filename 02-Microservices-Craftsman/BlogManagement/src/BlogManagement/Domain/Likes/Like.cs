namespace BlogManagement.Domain.Likes;

using System.ComponentModel.DataAnnotations;
using BlogManagement.Domain.PostAboutDrivers;
using BlogManagement.Domain.Drivers;
using System.ComponentModel.DataAnnotations.Schema;
using Destructurama.Attributed;
using BlogManagement.Exceptions;
using BlogManagement.Domain.Likes.Models;
using BlogManagement.Domain.Likes.DomainEvents;
using BlogManagement.Domain.Drivers;
using BlogManagement.Domain.Drivers.Models;
using BlogManagement.Domain.PostAboutDrivers;
using BlogManagement.Domain.PostAboutDrivers.Models;


public class Like : BaseEntity
{
    public string Info { get; private set; }

    public Driver Driver { get; private set; }

    public PostAboutDriver PostAboutDriver { get; private set; }

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static Like Create(LikeForCreation likeForCreation)
    {
        var newLike = new Like();

        newLike.Info = likeForCreation.Info;

        newLike.QueueDomainEvent(new LikeCreated(){ Like = newLike });
        
        return newLike;
    }

    public Like Update(LikeForUpdate likeForUpdate)
    {
        Info = likeForUpdate.Info;

        QueueDomainEvent(new LikeUpdated(){ Id = Id });
        return this;
    }

    public Like SetDriver(Driver driver)
    {
        Driver = driver;
        return this;
    }

    public Like SetPostAboutDriver(PostAboutDriver postAboutDriver)
    {
        PostAboutDriver = postAboutDriver;
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected Like() { } // For EF + Mocking
}
