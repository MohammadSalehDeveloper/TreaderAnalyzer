using AutoMapper;
using Core.Contracts.DTOs.Users;
using Core.Contracts.Interfaces.Persistence;
using MediatR;

namespace Core.Application.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUserRepository _users;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IUserRepository users, IMapper mapper)
    {
        _users = users;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _users.ListAsync(request.Role, request.Status, cancellationToken);
        return _mapper.Map<IReadOnlyList<UserDto>>(users);
    }
}
