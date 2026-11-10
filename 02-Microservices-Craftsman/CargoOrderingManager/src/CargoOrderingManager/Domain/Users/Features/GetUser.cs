namespace CargoOrderingManager.Domain.Users.Features;

using CargoOrderingManager.Domain.Users.Dtos;
using CargoOrderingManager.Domain.Users.Services;
using CargoOrderingManager.Exceptions;
using CargoOrderingManager.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetUser
{
    public sealed record Query(Guid UserId) : IRequest<UserDto>;

    public sealed class Handler : IRequestHandler<Query, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IUserRepository userRepository, IHeimGuardClient heimGuard)
        {
            _userRepository = userRepository;
            _heimGuard = heimGuard;
        }

        public async Task<UserDto> Handle(Query request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadUsers);

            var result = await _userRepository.GetById(request.UserId, cancellationToken: cancellationToken);
            return result.ToUserDto();
        }
    }
}