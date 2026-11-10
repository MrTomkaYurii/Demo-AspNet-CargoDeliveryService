namespace CargoOrderingManager.Domain.Users.Features;

using CargoOrderingManager.Domain.Users.Services;
using CargoOrderingManager.Domain.Users;
using CargoOrderingManager.Domain.Users.Dtos;
using CargoOrderingManager.Domain.Users.Models;
using CargoOrderingManager.Services;
using CargoOrderingManager.Exceptions;
using CargoOrderingManager.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddUser
{
    public sealed record Command(UserForCreationDto UserToAdd, bool SkipPermissions = false) : IRequest<UserDto>;

    public sealed class Handler : IRequestHandler<Command, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IUserRepository userRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task<UserDto> Handle(Command request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] if(!request.SkipPermissions)
                // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddUsers);

            var userToAdd = request.UserToAdd.ToUserForCreation();
            var user = User.Create(userToAdd);
            await _userRepository.Add(user, cancellationToken);

            await _unitOfWork.CommitChanges(cancellationToken);

            var userAdded = await _userRepository.GetById(user.Id, cancellationToken: cancellationToken);
            return userAdded.ToUserDto();
        }
    }
}
