using Core.Contracts.DTOs.Users;
using MediatR;

namespace Core.Application.Users.Queries.GetBalanceByUserId;

public sealed record GetBalanceByUserIdQuery(Guid UserId) : IRequest<BalanceDto>;
