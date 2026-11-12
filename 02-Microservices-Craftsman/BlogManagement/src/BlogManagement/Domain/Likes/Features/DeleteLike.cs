namespace BlogManagement.Domain.Likes.Features;

using BlogManagement.Domain.Likes.Services;
using BlogManagement.Services;
using BlogManagement.Exceptions;
using BlogManagement.Domain;
using HeimGuard;
using MediatR;

public static class DeleteLike
{
    public sealed record Command(Guid LikeId) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
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

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeleteLike);

            var recordToDelete = await _likeRepository.GetById(request.LikeId, cancellationToken: cancellationToken);
            _likeRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}