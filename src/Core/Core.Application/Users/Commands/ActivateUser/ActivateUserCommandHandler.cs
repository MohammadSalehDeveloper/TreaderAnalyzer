using Core.Application.Common.Exceptions;
using Core.Contracts.Interfaces.Persistence;
using MediatR;

namespace Core.Application.Users.Commands.ActivateUser;

public sealed class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        user.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
