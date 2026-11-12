namespace BlogManagement.Domain.PostAboutDrivers;

using System.ComponentModel.DataAnnotations;
using BlogManagement.Domain.BlogUsers;
using BlogManagement.Domain.Comments;
using BlogManagement.Domain.Drivers;
using BlogManagement.Domain.Likes;
using System.ComponentModel.DataAnnotations.Schema;
using Destructurama.Attributed;
using BlogManagement.Exceptions;
using BlogManagement.Domain.PostAboutDrivers.Models;
using BlogManagement.Domain.PostAboutDrivers.DomainEvents;


public class PostAboutDriver : BaseEntity
{
    public string Title { get; private set; }

    public string Content { get; private set; }

    public IReadOnlyCollection<Like> Likes { get; } = new List<Like>();

    public Driver Driver { get; }

    public IReadOnlyCollection<Comment> Comments { get; } = new List<Comment>();

    public BlogUser BlogUser { get; }

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static PostAboutDriver Create(PostAboutDriverForCreation postAboutDriverForCreation)
    {
        var newPostAboutDriver = new PostAboutDriver();

        newPostAboutDriver.Title = postAboutDriverForCreation.Title;
        newPostAboutDriver.Content = postAboutDriverForCreation.Content;

        newPostAboutDriver.QueueDomainEvent(new PostAboutDriverCreated(){ PostAboutDriver = newPostAboutDriver });
        
        return newPostAboutDriver;
    }

    public PostAboutDriver Update(PostAboutDriverForUpdate postAboutDriverForUpdate)
    {
        Title = postAboutDriverForUpdate.Title;
        Content = postAboutDriverForUpdate.Content;

        QueueDomainEvent(new PostAboutDriverUpdated(){ Id = Id });
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected PostAboutDriver() { } // For EF + Mocking
}
