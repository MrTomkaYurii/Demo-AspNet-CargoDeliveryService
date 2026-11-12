namespace BlogManagement.Domain.BlogUsers.Features;

using BlogManagement.Domain.BlogUsers.Services;
using BlogManagement.Services;
using BlogManagement.Exceptions;
using BlogManagement.Domain;
using HeimGuard;
using MediatR;

public static class DeleteBlogUser
{
    public sealed record Command(Guid BlogUserId) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
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

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeleteUser);

            var recordToDelete = await _blogUserRepository.GetById(request.BlogUserId, cancellationToken: cancellationToken);
            _blogUserRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}