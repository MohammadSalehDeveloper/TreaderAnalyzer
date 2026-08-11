using FluentValidation;

namespace Core.Application.Users.Queries.GetBalanceByUserId;

public sealed class GetBalanceByUserIdQueryValidator : AbstractValidator<GetBalanceByUserIdQuery>
{
    public GetBalanceByUserIdQueryValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
    }
}
