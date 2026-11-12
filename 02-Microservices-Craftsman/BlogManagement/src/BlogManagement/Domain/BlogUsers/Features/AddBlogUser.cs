namespace BlogManagement.Domain.BlogUsers.Features;

using BlogManagement.Domain.BlogUsers.Services;
using BlogManagement.Domain.BlogUsers;
using BlogManagement.Domain.BlogUsers.Dtos;
using BlogManagement.Domain.BlogUsers.Models;
using BlogManagement.Services;
using BlogManagement.Exceptions;
using BlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddBlogUser
{
    public sealed record Command(BlogUserForCreationDto BlogUserToAdd) : IRequest<BlogUserDto>;

    public sealed class Handler : IRequestHandler<Command, BlogUserDto>
    {
        private readonly IBlogUserRepository _blogUserRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IBlogUserRepository blogUserRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _blogUserRepository = blogUserRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task<BlogUserDto> Handle(Command request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddUser);

            var blogUserToAdd = request.BlogUserToAdd.ToBlogUserForCreation();
            var blogUser = BlogUser.Create(blogUserToAdd);

            await _blogUserRepository.Add(blogUser, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return blogUser.ToBlogUserDto();
        }
    }
}