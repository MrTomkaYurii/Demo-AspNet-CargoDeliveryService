namespace BlogManagement.Domain.PostAboutDrivers.Features;

using BlogManagement.Domain.PostAboutDrivers.Services;
using BlogManagement.Domain.PostAboutDrivers;
using BlogManagement.Domain.PostAboutDrivers.Dtos;
using BlogManagement.Domain.PostAboutDrivers.Models;
using BlogManagement.Services;
using BlogManagement.Exceptions;
using BlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddPostAboutDriver
{
    public sealed record Command(PostAboutDriverForCreationDto PostAboutDriverToAdd) : IRequest<PostAboutDriverDto>;

    public sealed class Handler : IRequestHandler<Command, PostAboutDriverDto>
    {
        private readonly IPostAboutDriverRepository _postAboutDriverRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IPostAboutDriverRepository postAboutDriverRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _postAboutDriverRepository = postAboutDriverRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task<PostAboutDriverDto> Handle(Command request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddPostAboutDriver);

            var postAboutDriverToAdd = request.PostAboutDriverToAdd.ToPostAboutDriverForCreation();
            var postAboutDriver = PostAboutDriver.Create(postAboutDriverToAdd);

            await _postAboutDriverRepository.Add(postAboutDriver, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return postAboutDriver.ToPostAboutDriverDto();
        }
    }
}