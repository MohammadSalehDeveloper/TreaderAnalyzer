using AutoMapper;
using Core.Application.Common.Exceptions;
using Core.Contracts.DTOs.Users;
using Core.Contracts.Interfaces.Persistence;
using MediatR;

namespace Core.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IUserRepository users, IMapper mapper)
    {
        _users = users;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdWithBalanceAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        return _mapper.Map<UserDto>(user);
    }
}
