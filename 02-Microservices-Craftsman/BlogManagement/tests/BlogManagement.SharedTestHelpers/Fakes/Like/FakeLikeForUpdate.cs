namespace BlogManagement.SharedTestHelpers.Fakes.Like;

using AutoBogus;
using BlogManagement.Domain.Likes;
using BlogManagement.Domain.Likes.Models;

public sealed class FakeLikeForUpdate : AutoFaker<LikeForUpdate>
{
    public FakeLikeForUpdate()
    {
    }
}