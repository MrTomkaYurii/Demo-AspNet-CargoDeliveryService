namespace BlogManagement.Domain.Likes.Features;

using BlogManagement.Domain.Likes.Services;
using BlogManagement.Domain.Likes;
using BlogManagement.Domain.Likes.Dtos;
using BlogManagement.Domain.Likes.Models;
using BlogManagement.Services;
using BlogManagement.Exceptions;
using BlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddLike
{
    public sealed record Command(LikeForCreationDto LikeToAdd) : IRequest<LikeDto>;

    public sealed class Handler : IRequestHandler<Command, LikeDto>
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ILikeRepository likeRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _likeRepository = likeRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task<LikeDto> Handle(Command request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddLike);

            var likeToAdd = request.LikeToAdd.ToLikeForCreation();
            var like = Like.Create(likeToAdd);

            await _likeRepository.Add(like, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return like.ToLikeDto();
        }
    }
}