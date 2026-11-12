namespace BlogManagement.Domain.BlogUsers;

using System.ComponentModel.DataAnnotations;
using BlogManagement.Domain.PostAboutDrivers;
using BlogManagement.Domain.PostAboutDrivers.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Destructurama.Attributed;
using BlogManagement.Exceptions;
using BlogManagement.Domain.BlogUsers.Models;
using BlogManagement.Domain.BlogUsers.DomainEvents;
using BlogManagement.Domain.PostAboutDrivers;
using BlogManagement.Domain.PostAboutDrivers.Models;


public class BlogUser : BaseEntity
{
    public string UserName { get; private set; }

    public string Email { get; private set; }

    // Без ініціалізатора: інакше кожен новий користувач блогу створював би порожній пост.
    public PostAboutDriver PostAboutDriver { get; private set; }

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static BlogUser Create(BlogUserForCreation blogUserForCreation)
    {
        var newBlogUser = new BlogUser();

        newBlogUser.UserName = blogUserForCreation.UserName;
        newBlogUser.Email = blogUserForCreation.Email;

        newBlogUser.QueueDomainEvent(new BlogUserCreated(){ BlogUser = newBlogUser });
        
        return newBlogUser;
    }

    public BlogUser Update(BlogUserForUpdate blogUserForUpdate)
    {
        UserName = blogUserForUpdate.UserName;
        Email = blogUserForUpdate.Email;

        QueueDomainEvent(new BlogUserUpdated(){ Id = Id });
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected BlogUser() { } // For EF + Mocking
}
