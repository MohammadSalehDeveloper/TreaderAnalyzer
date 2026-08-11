using AutoMapper;
using Core.Application.Common.Exceptions;
using Core.Contracts.DTOs.Users;
using Core.Contracts.Interfaces.Persistence;
using MediatR;

namespace Core.Application.Users.Queries.GetBalanceByUserId;

public sealed class GetBalanceByUserIdQueryHandler : IRequestHandler<GetBalanceByUserIdQuery, BalanceDto>
{
    private readonly IBalanceRepository _balances;
    private readonly IMapper _mapper;

    public GetBalanceByUserIdQueryHandler(IBalanceRepository balances, IMapper mapper)
    {
        _balances = balances;
        _mapper = mapper;
    }

    public async Task<BalanceDto> Handle(GetBalanceByUserIdQuery request, CancellationToken cancellationToken)
    {
        var balance = await _balances.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Balance), request.UserId);

        return _mapper.Map<BalanceDto>(balance);
    }
}
